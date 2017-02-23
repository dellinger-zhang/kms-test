using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webrtctest.kms.model
{
    public class PipelineModel : BaseModel
    {
        private static readonly string FMT_CREATE = 
        @"{
            ""id"": {id},
            ""method"": ""create"",
            ""params"": {
                ""type"": ""MediaPipeline"",
                ""constructorParams"": {},
                ""properties"": {}
            },
            ""jsonrpc"": ""{jsonrpc}""
        }";

        private static readonly string FMT_RELEASE =
        @"{
            ""id"": {id},
            ""method"": ""release"",
            ""params"": {
                ""object"": ""{pipelineId}"",
                ""sessionId"": ""{sessionId}""
            },
            ""jsonrpc"": ""{jsonrpc}""
        }";

        private string fmt_string = string.Empty;
        public PipelineModel Create()
        {
            fmt_string = FMT_CREATE;
            id = GetId();
            return this;
        }

        public PipelineModel Release()
        {
            fmt_string = FMT_RELEASE;
            id = GetId();
            return this;
        }

        public string pipelineId { get; set; }
        public string sessionId { get; set; }

        public override string ToJsonString()
        {
            return fmt_string.Replace("{id}", id.ToString())
                   .Replace("{jsonrpc}", jsonrpc)
                   .Replace("{pipelineId}", pipelineId)
                   .Replace("{sessionId}", sessionId);
        }
    }
}
