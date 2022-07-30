using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PaymentWeb.Services;
using Server.Common;
using Database.Models;

namespace PaymentWeb.Pages
{
    public class VnPayReturnModel : PageModel
    {
        private readonly PaymentService _paymentService;
        public VnPayReturnModel(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [BindProperty(SupportsGet = true)]
        public string vnp_TmnCode { get; set; } = "";
        [BindProperty(SupportsGet = true)]
        public double vnp_Amount { get; set; }
        [BindProperty(SupportsGet = true)]
        public string vnp_BankCode { get; set; } = "";
        [BindProperty(SupportsGet = true)]
        public string vnp_BankTranNo { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public string vnp_CardType { get; set; } = "";
        [BindProperty(SupportsGet = true)]
        public string vnp_PayDate { get; set; } = "";   //yyyyMMddHHmmss
        [BindProperty(SupportsGet = true)]
        public string vnp_TransactionNo { get; set; } = ""; // VnPay transactionNo
        [BindProperty(SupportsGet = true)]
        public string vnp_ResponseCode { get; set; } = ""; //00: Success
        [BindProperty(SupportsGet = true)]
        public string vnp_TransactionStatus { get; set; } = ""; //00: Success
        [BindProperty(SupportsGet = true)]
        public string vnp_TxnRef { get; set; } = ""; //TransactionID from merchant
        [BindProperty(SupportsGet = true)]
        public string vnp_SecureHashType { get; set; } = ""; //Hash type
        [BindProperty(SupportsGet = true)]
        public string vnp_SecureHash { get; set; } = ""; //Hash type
        [BindProperty]
        public string ErrorMessage { get; set; } = "";
        [BindProperty]
        public int ReturnCode { get; set; } = 200;
        [BindProperty]
        public mdSaleOrder Record { get; set; } = new mdSaleOrder();

        //
        public async Task<IActionResult> OnGet()
        {
            try
            {
                var result = new VnPayResult();
                result.vnp_TmnCode = vnp_TmnCode;
                result.vnp_Amount = vnp_Amount;
                result.vnp_BankCode = vnp_BankCode;
                result.vnp_BankTranNo = vnp_BankTranNo;
                result.vnp_CardType = vnp_CardType;
                result.vnp_PayDate = vnp_PayDate;
                result.vnp_TransactionNo = vnp_TransactionNo;
                result.vnp_ResponseCode = vnp_ResponseCode;
                result.vnp_TransactionStatus = vnp_TransactionStatus;
                result.vnp_TxnRef = vnp_TxnRef;
                result.vnp_SecureHash = vnp_SecureHash;

                //get all querystring data
                var querryData = new Dictionary<string, string>();
                foreach (var param in HttpContext.Request.Query)
                {
                    //get all querystring data
                    if (!string.IsNullOrEmpty(param.Key) && param.Key.StartsWith("vnp_"))
                    {
                        querryData.Add(param.Key, param.Value);
                    }
                }

                //Get base Url
                MyData.BaseUrl = @$"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";

                //Finish payment
                var ret = await _paymentService.FinishPayment(MyConstant.PaymentChannel_VnPay, result, querryData);
                //
                ReturnCode = ret.ReturnCode;
                ErrorMessage = ret.ErrorMessage;
                Record = ret.Record;
            }
            catch
            {
                ReturnCode = 500;
                ErrorMessage = "Server đang tạm ngưng phục vụ, xin hãy quay lại sau.";
            }
            //
            return Page();
        }
    }
}
