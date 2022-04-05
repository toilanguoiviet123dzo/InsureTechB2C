using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PaymentWeb.Pages
{
    public class TestModel : PageModel
    {

        [BindProperty(SupportsGet = true)]
        public string UserMessage { get; set; } = "";
        [BindProperty(SupportsGet = true)]
        public string OtherMessage { get; set; } = "";
        [BindProperty(SupportsGet = true)]
        public string ChannelID { get; set; } = "";
        public void OnGet()
        {
            ViewData["Message"] = "xxxxxxxxxx";
            ViewData["UserMessage"] = UserMessage;
            ViewData["OtherMessage"] = OtherMessage;
            ViewData["ChannelID"] = ChannelID;
        }
    }
}
