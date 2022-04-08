using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eBaoIssuanceAPI
{
    class PrintListModel
    {
        public class DataPrintListModel
        {
            public string prdtCode;
            public List<string> printFileTypes;
        }

        public bool success;
        public string errorCode;
        public List<DataPrintListModel> data;
    }
}
