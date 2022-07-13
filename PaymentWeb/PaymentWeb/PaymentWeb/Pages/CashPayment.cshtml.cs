using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PaymentWeb.Services;
using BlazorApp.Server.Common;
using BlazorApp.Server.Models;

namespace PaymentWeb.Pages
{
    public class CashPayment : PageModel
    {
        private readonly PaymentService _paymentService;
        public CashPayment(PaymentService paymentService)
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
                var ret = await _paymentService.DonateOrder(InitOrderToken, TransactionID);
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
