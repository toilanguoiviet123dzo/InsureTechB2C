using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PaymentWeb.Services;
using Server.Common;
using Database.Models;

namespace PaymentWeb.Pages
{
    public class CashPaymentModel : PageModel
    {
        private readonly PaymentService _paymentService;
        public CashPaymentModel(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }
        //Params
        [BindProperty(SupportsGet = true)]
        public string InitOrderToken { get; set; } = "";
        [BindProperty(SupportsGet = true)]
        public string TransactionID { get; set; } = "";

        [BindProperty]
        public string ErrorMessage { get; set; } = "";
        [BindProperty]
        public int ReturnCode { get; set; }
        [BindProperty]
        public mdSaleOrder Record { get; set; } = new mdSaleOrder();

        //
        public async Task<IActionResult> OnGet()
        {
            try
            {
                //Get base Url
                MyData.BaseUrl = @$"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";

                //Finish
                var ret = await _paymentService.CashPayment(InitOrderToken, TransactionID);
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
