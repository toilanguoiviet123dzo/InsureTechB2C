using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eBaoIssuanceAPI
{

    class DataModel
    {
        public string processStatus;
        public string processMsg;
        public string policyId;
        public string quoteNo;
        public string policyNo;
        public string paymentRedirectURL;
        public List<string> subPolicyResults;
    }
    class IssueResponse
    {
        public DataModel data;
    }
}
