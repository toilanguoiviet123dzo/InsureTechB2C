using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PaymentWeb.Services;
using BlazorApp.Server.Common;
using BlazorApp.Server.Models;

namespace PaymentWeb.Pages
{
    public class VnPayReturnModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string vnp_TmnCode { get; set; } = "";
        [BindProperty(SupportsGet = true)]
        public string vnp_Amount { get; set; } = "";
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
        public int ReturnCode { get; set; }
        [BindProperty]
        public mdSaleOrder Record { get; set; } = new mdSaleOrder();

        //
        public async void OnGet()
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
            result.vnp_SecureHashType = vnp_SecureHashType;
            result.vnp_SecureHash = vnp_SecureHash;
            //Finish payment
            var ret = await PaymentService.FinishVnPay(result);
            //
            ReturnCode = ret.ReturnCode;
            ErrorMessage = ret.ErrorMessage;
            Record = ret.Record;
        }
    }
}
