
namespace Server.Common
{
    public class CallApiReturn
    {
        public int ReturnCode = 200;
        public string ErrorMessage = "";
        public dynamic? Data { get; set; }
    }
}
