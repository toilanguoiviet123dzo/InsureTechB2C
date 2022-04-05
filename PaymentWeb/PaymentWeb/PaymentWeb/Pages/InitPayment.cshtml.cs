using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PaymentWeb.Services;
using BlazorApp.Server.Common;
using BlazorApp.Server.Models;

namespace PaymentWeb.Pages
{
    public class InitPaymentModel : PageModel
    {
        private IHttpContextAccessor _accessor;
        public InitPaymentModel(IHttpContextAccessor httpContextAccessor)
        {
            _accessor = httpContextAccessor;
        }
        //Params
        [BindProperty(SupportsGet = true)]
        public string InitOrderToken { get; set; } = "";
        [BindProperty(SupportsGet = true)]
        public string TransactionID { get; set; } = "";
        [BindProperty(SupportsGet = true)]
        public string PaymentChannel { get; set; } = "";

        //View data
        [BindProperty]
        public string ErrorMessage { get; set; } = "";

        public async Task<IActionResult> OnGet()
        {
            // InitPayment
            var ret = await PaymentService.InitPayment(InitOrderToken, TransactionID);
            if (ret.ReturnCode != ReturnCode.OK)
            {
                //Error_201
                if (ret.ReturnCode == ReturnCode.Error_201) ErrorMessage = "Lỗi bảo mật";
                //Error_202
                if (ret.ReturnCode == ReturnCode.Error_202) ErrorMessage = "Không tồn tại giao dịch tương ứng";
                //Error_ByServer
                if (ret.ReturnCode == ReturnCode.Error_ByServer) ErrorMessage = "Server tạm thời ngưng phục vụ, xin vui lòng quay lại sau.";

                //Show error on this page
                return Page();
            }



            //Create redirect payment
            string ip = "";
            if (_accessor.HttpContext != null 
                && _accessor.HttpContext.Connection != null
                && _accessor.HttpContext.Connection.RemoteIpAddress != null) ip = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
            //
            var url = await PaymentService.Gen_PaymentRedirectLink(PaymentChannel, ip, ret.Record);
            //
            return Redirect(url);
        }
    }
}
