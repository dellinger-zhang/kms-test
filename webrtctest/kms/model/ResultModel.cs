using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webrtctest.kms.model
{
    public class ResultModel : BaseModel
    {
        public IDictionary<string, object> result { get; set; }

        public override string ToJsonString()
        {
            throw new NotImplementedException();
        }
    }
}
