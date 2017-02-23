using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace webrtctest.kms.model
{
    public abstract class BaseModel
    {
        private static int __id = 1;
        public BaseModel()
        {
            jsonrpc = "2.0";
        }

        public static int GetId()
        {
            return Interlocked.Increment(ref __id);
        }

        public string jsonrpc { get; set; }

        public int id { get; set; }

        public abstract string ToJsonString();
    }
}
