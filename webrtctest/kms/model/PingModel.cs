using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webrtctest.kms.model
{
    public class PingModel : BaseModel
    {
        public PingModel()
        {
            interval = 240000;
        }

        public int interval { get; set; }

        public override string ToJsonString()
        {
            return @"{
                        ""id"": " + GetId() + @",
                        ""method"": ""ping"",
                        ""params"": {
                            ""interval"": " + interval + @"
                        },
                        ""jsonrpc"": """ + jsonrpc + @"""
                    }";
        }
    }
}
