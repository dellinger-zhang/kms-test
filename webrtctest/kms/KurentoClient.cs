using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebRtc.NET;
using webrtctest.kms.model;

namespace webrtctest.kms
{
    public class KurentoClient : IDisposable
    {
        delegate void CallBackInvoke (ResultModel result);
        private readonly ClientWebSocket client;
        private PipelineModel pipeline;
        private WebRtcEndpointModel webrtcEp;
        private readonly IDictionary<int, CallBackInvoke> callbacks;
        private readonly ManagedConductor condutor;

        private string localSdp = string.Empty;
        private string remoteSdp = string.Empty;
        private readonly TurboJpegEncoder encoder = TurboJpegEncoder.CreateEncoder();

        public Action<KurentoClient> OnReady;

        class InnerCandidate
        {
            public string sdp_mid { get; set; }
            public int sdp_mline_index { get; set; }
            public string sdp { get; set; }
        }

        List<InnerCandidate> candidates = new List<InnerCandidate>();

        public KurentoClient(string url)
        {
            condutor = new ManagedConductor();
            condutor.SetAudio(false);
            condutor.SetVideoCapturer(640, 480, 5, true);
            condutor.OnSuccessOffer += Condutor_OnSuccessOffer;
            condutor.OnSuccessAnswer += Condutor_OnSuccessOffer;
            condutor.OnIceCandidate += Condutor_OnIceCandidate;

            unsafe
            {
                condutor.OnRenderLocal += Condutor_OnRenderLocal;
                condutor.OnRenderRemote += Condutor_OnRenderRemote;
            }
            callbacks = new Dictionary<int, CallBackInvoke>();
            client = new ClientWebSocket();
            client.ConnectAsync(new Uri(url), new CancellationToken()).Wait();
            Task receiving = Task.Factory.StartNew(_Receive, TaskCreationOptions.LongRunning);
        }

        private void Condutor_OnIceCandidate(string sdp_mid, int sdp_mline_index, string sdp)
        {
            Console.WriteLine("sdp_mid:" + sdp_mid + " index:" + sdp_mline_index + " sdp:" + sdp);
            //condutor.AddIceCandidate(sdp_mid, sdp_mline_index, sdp);
            candidates.Add(new InnerCandidate() {
                sdp_mid = sdp_mid,
                sdp_mline_index = sdp_mline_index,
                sdp = sdp
            });
            condutor.ProcessMessages(1000);
        }

        private void Condutor_OnSuccessOffer(string sdp)
        {
            //ManagedConductor.InitializeSSL();
            //Console.WriteLine("sdp:" + sdp);
            if (webrtcEp != null)
            {
                localSdp = sdp;
                
                var json = webrtcEp.ProcessOffer(pipeline, sdp).ToJsonString();
                _Send(json);
                callbacks.Add(webrtcEp.id, (ret) => {
                    if (ret.result != null)
                    {
                        remoteSdp = (string) ret.result["value"];
                    }
                    else
                    {
                        Console.WriteLine("fail to get sdp!!");
                        return;
                    }
                    Console.WriteLine("Remote sdp:" + remoteSdp);
                    callbacks.Remove(webrtcEp.id);
                    
                    condutor.OnOfferReply("answer", remoteSdp);
                    condutor.ProcessMessages(1000);
                    json = webrtcEp.GatherCandidates(pipeline).ToJsonString();
                    _Send(json);
                    condutor.ProcessMessages(1000);

                    foreach (var candidate in candidates)
                    {
                        //condutor.AddIceCandidate(candidate.sdp_mid, candidate.sdp_mline_index, candidate.sdp);
                        var jsonText = webrtcEp.AddIceCandidate(pipeline, candidate.sdp, candidate.sdp_mid, candidate.sdp_mline_index).ToJsonString();
                        _Send(jsonText);
                    }
                    
                    condutor.ProcessMessages(1000);
                    if (OnReady != null)
                    {
                        OnReady(this);
                    }
                });
                
            }
        }

        private unsafe void Condutor_OnRenderRemote(byte* frame_buffer, uint w, uint h)
        {
            Console.WriteLine("remote picture...");
        }

        private unsafe void Condutor_OnRenderLocal(byte* frame_buffer, uint w, uint h)
        {
            //TODO:
        }

        public unsafe void SendVideo(byte *pImg, int screenWidth, int screenHeight)
        {
            if (pImg == null) return;

            unsafe
            {
                var yuv = condutor.VideoCapturerI420Buffer();
                if (yuv != null)
                {
                    encoder.EncodeBGR24toI420(pImg, screenWidth, screenHeight, yuv, 0, true);
                }
                condutor.PushFrame();
            }
        }

        public void Ping()
        {
            _Send(new PingModel().ToJsonString());
        }

        public void CreatePipeline()
        {
            pipeline = new PipelineModel();
            var json = pipeline.Create().ToJsonString();
            _Send(json);
            callbacks.Add(pipeline.id, (ret) => {
                pipeline.pipelineId = (string) ret.result["value"];
                pipeline.sessionId = (string)ret.result["sessionId"];
                Console.WriteLine("OK, here. " + JsonConvert.SerializeObject(pipeline));
                callbacks.Remove(pipeline.id);
                CreateLookbackWebrtcEndpoint();
            });
        }

        public void CreateLookbackWebrtcEndpoint()
        {
            if (pipeline == null)
            {
                return;
            }
            webrtcEp = new WebRtcEndpointModel();
            var json = webrtcEp.Create(pipeline).ToJsonString();
            _Send(json);
            callbacks.Add(webrtcEp.id, (ret) => {
                webrtcEp.WebrtcId = (string) ret.result["value"];
                callbacks.Remove(webrtcEp.id);

                var json2 = webrtcEp.Connect(pipeline, webrtcEp, webrtcEp)
                    .ToJsonString();
                _Send(json2);
                callbacks.Add(webrtcEp.id, (ret2) =>
                {
                    ManagedConductor.InitializeSSL();
                    condutor.AddServerConfig("stun:10.30.29.122:5766", string.Empty, string.Empty);
                    condutor.AddServerConfig("turn:10.30.29.122:3478", "kurento", "kurento");
                    condutor.ProcessMessages(1000);
                    condutor.InitializePeerConnection();
                    condutor.CreateOffer();
                    callbacks.Remove(webrtcEp.id);
                    condutor.ProcessMessages(1000);
                    json2 = webrtcEp.subscribe(pipeline, "IceCandidateFound").ToJsonString();
                    _Send(json2);
                });

            });



        }

        void _Send(string msg)
        {
            Console.WriteLine("send:" + msg);
            client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg)),
                WebSocketMessageType.Text, true, CancellationToken.None).Wait();
        }

        public void OnMessage(string message)
        {
            Console.WriteLine("GOT:"  + message);
            ResultModel ret = null;
            try
            {
                ret = JsonConvert.DeserializeObject<ResultModel>(message);
                if (callbacks.ContainsKey(ret.id))
                {
                    callbacks[ret.id](ret);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
        }

        void OnEvent(IDictionary<string, object> map)
        {
            if (map == null || map.Count < 1)
            {
                return;
            }

            IDictionary<string, string> candidate = ((IDictionary<string, IDictionary<string, IDictionary<string, IDictionary<string, string>>>>) map["params"])["value"]["data"]["candidate"];
            condutor.AddIceCandidate(candidate["sdpMid"], int.Parse(candidate["sdpMLineIndex"]), candidate["candidate"]);
        }

        void _Receive()
        {
            byte[] readBuffer = new byte[10240];
            
            while (true)
            {
                client.ReceiveAsync(new ArraySegment<byte>(readBuffer), CancellationToken.None).Wait();

                string message = Encoding.UTF8.GetString(readBuffer);
                message = message.Substring(0, message.IndexOf('\0'));
                if (message.IndexOf("\"method\":\"onEvent\"") > -1)
                {
                    IDictionary<string, object> map;
                    try
                    {
                        map = JsonConvert.DeserializeObject<IDictionary<string, object>>(message);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex.Message);
                    }
                }
                else
                {
                    OnMessage(message);
                }
                
                readBuffer = new byte[10240];
            }
        }

        public void Dispose()
        {
            if (pipeline != null)
            {
                pipeline.Release();
            }

            if (client != null)
            {
                client.Dispose();
            }
        }
    }
}
