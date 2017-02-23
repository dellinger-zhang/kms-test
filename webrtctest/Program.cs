using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRtc.NET;
using webrtctest.kms;
using System.Timers;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.IO;

namespace webrtctest
{
    class Program
    {
        static readonly int screenWidth = 640, screenHeight = 480;
        //static readonly Timer timer = new Timer();
        static Bitmap img;
        static Graphics g;
        static readonly byte[] imgBuf = new byte[screenWidth * 3 * screenHeight];
        static IntPtr imgBufPtr = IntPtr.Zero;
        static Font font;

        static void PrepareImage()
        {
            font = new Font("Tahoma", 36);
            var bufHandle = GCHandle.Alloc(imgBuf, GCHandleType.Pinned);
            imgBufPtr = bufHandle.AddrOfPinnedObject();
            img = new Bitmap(screenWidth, screenHeight, screenWidth * 3, PixelFormat.Format24bppRgb, imgBufPtr);
           
            g = Graphics.FromImage(img);
        }

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: webrtctest.exe logpath");
                return;
            }
            FileStream fs = new FileStream(args[0], FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            Console.SetOut(new StreamWriter(fs));
            PrepareImage();

            using (KurentoClient client = new KurentoClient("ws://10.30.29.122:8888/kurento"))
            {
                Timer timer = new Timer();
                client.Ping();
                client.CreatePipeline();

                client.OnReady = (kClient) =>
                {
                    timer.Interval = 200;
                    timer.Elapsed += delegate (object obj, ElapsedEventArgs e) {
                        g.Clear(Color.DeepSkyBlue);
                        g.DrawString(DateTime.Now.ToString("hh:mm:ss.fff"), font, Brushes.LimeGreen, 0.0f, 0.0f, StringFormat.GenericDefault);
                        unsafe
                        {
                            kClient.SendVideo((byte*)imgBufPtr.ToPointer(), screenWidth, screenHeight);
                        }
                        
                    };
                    timer.Enabled = true;
                    
                };
                Console.ReadLine();
            }
        }
    }
}
