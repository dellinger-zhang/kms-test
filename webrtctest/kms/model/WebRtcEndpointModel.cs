using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webrtctest.kms.model
{
    public class WebRtcEndpointModel : BaseModel
    {
        private static readonly string FMT_CREATE =
        @"{
            ""id"": {id},
            ""method"": ""create"",
            ""params"": {
                ""type"": ""WebRtcEndpoint"",
                ""constructorParams"": {
                    ""mediaPipeline"": ""{pipelineId}""
                },
                ""properties"": {},
                ""sessionId"": ""{sessionId}""
            },
            ""jsonrpc"": ""{jsonrpc}""
        }";

        private static readonly string FMT_CONNECT =
        @"{
            ""id"": {id},
            ""method"": ""invoke"",
            ""params"": {
                ""object"": ""{sourceId}"",
                ""operation"": ""connect"",
                ""operationParams"": {
                    ""sink"": ""{sinkId}""
                },
                ""sessionId"": ""{sessionId}""
            },
            ""jsonrpc"": ""{jsonrpc}""
        }";

        private static readonly string FMT_PROC_OFFER =
        @"{
              ""id"": {id},
              ""method"":""invoke"",
              ""params"":{
                 ""object"":""{endpointId}"",
                 ""operation"":""processOffer"",
                 ""operationParams"":{
                    ""offer"":""{sdp}""
                 },
                 ""sessionId"":""{sessionId}""
              },
              ""jsonrpc"":""{jsonrpc}""
        }";

        private static readonly string FMT_ADDICECANDIDATE =
        @"{
              ""id"": {id},
              ""method"":""invoke"",
              ""params"":{
                 ""object"":""{endpointId}"",
                 ""operation"":""addIceCandidate"",
                 ""operationParams"":{
                    ""candidate"": {
                        ""__module__"": ""kurento"",
                        ""__type__"": ""IceCandidate"",
                        ""candidate"":""{candidate}"",
                        ""sdpMid"": ""{sdpMid}"",
                        ""sdpMLineIndex"": {sdpMLineIndex}
                    }
                 },
                 ""sessionId"":""{sessionId}""
              },
              ""jsonrpc"":""{jsonrpc}""
        }";

        private static readonly string FMT_GATHERCANDIDATES =
        @"{
              ""id"": {id},
              ""method"":""invoke"",
              ""params"":{
                 ""object"":""{endpointId}"",
                 ""operation"":""gatherCandidates"",
                 ""sessionId"":""{sessionId}""
              },
              ""jsonrpc"":""{jsonrpc}""
        }";

        private static readonly string FMT_SUBSCRIBE =
        @"{
              ""id"": {id},
              ""method"":""subscribe"",
              ""params"":{
                 ""type"": ""{event}"",
                 ""object"":""{endpointId}"",
                 ""sessionId"":""{sessionId}""
              },
              ""jsonrpc"":""{jsonrpc}""
        }";

        private string fmt_string = string.Empty;
        public WebRtcEndpointModel Create(PipelineModel pipeline)
        {
            fmt_string = FMT_CREATE;
            id = GetId();
            fmt_string = fmt_string.Replace("{id}", id.ToString())
                .Replace("{pipelineId}", pipeline.pipelineId)
                .Replace("{sessionId}", pipeline.sessionId)
                .Replace("{jsonrpc}", jsonrpc);
            return this;
        }

        public WebRtcEndpointModel Connect(PipelineModel pipeline, WebRtcEndpointModel end1, WebRtcEndpointModel end2)
        {
            fmt_string = FMT_CONNECT;
            id = GetId();
            fmt_string = fmt_string.Replace("{id}", id.ToString())
                .Replace("{sourceId}", end1.WebrtcId)
                .Replace("{sinkId}", end2.WebrtcId)
                .Replace("{sessionId}", pipeline.sessionId)
                .Replace("{jsonrpc}", jsonrpc);
            return this;
        }

        public WebRtcEndpointModel ProcessOffer(PipelineModel pipeline, string sdp)
        {
            fmt_string = FMT_PROC_OFFER;
            id = GetId();
            sdp = sdp.Replace("\r\n", "\\r\\n");
            fmt_string = fmt_string.Replace("{id}", id.ToString())
                .Replace("{sdp}", sdp)
                .Replace("{sessionId}", pipeline.sessionId)
                .Replace("{endpointId}", WebrtcId)
                .Replace("{jsonrpc}", jsonrpc);
            
            return this;
        }

        public WebRtcEndpointModel AddIceCandidate(PipelineModel pipeline, string candidate, string sdpMid, int sdpIndex)
        {
            fmt_string = FMT_ADDICECANDIDATE;
            id = GetId();
            fmt_string = fmt_string.Replace("{id}", id.ToString())
                .Replace("{candidate}", candidate)
                .Replace("{sdpMid}", sdpMid)
                .Replace("{sdpMLineIndex}", sdpIndex.ToString())
                .Replace("{sessionId}", pipeline.sessionId)
                .Replace("{endpointId}", WebrtcId)
                .Replace("{jsonrpc}", jsonrpc);
            return this;
        }

        public WebRtcEndpointModel GatherCandidates(PipelineModel pipeline)
        {
            fmt_string = FMT_GATHERCANDIDATES;
            id = GetId();
            fmt_string = fmt_string.Replace("{id}", id.ToString())
                .Replace("{sessionId}", pipeline.sessionId)
                .Replace("{endpointId}", WebrtcId)
                .Replace("{jsonrpc}", jsonrpc);
            return this;
        }

        public WebRtcEndpointModel subscribe(PipelineModel pipeline, string evt)
        {
            fmt_string = FMT_SUBSCRIBE;
            id = GetId();
            fmt_string = fmt_string.Replace("{id}", id.ToString())
                .Replace("{event}", evt)
                .Replace("{sessionId}", pipeline.sessionId)
                .Replace("{endpointId}", WebrtcId)
                .Replace("{jsonrpc}", jsonrpc);
            return this;
        }

        public string WebrtcId { get; set; }

        public override string ToJsonString()
        {
            return fmt_string;
        }
    }
}
