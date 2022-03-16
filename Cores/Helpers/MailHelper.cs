using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cores.Helpers
{
    /// <summary>
    /// Lớp cung cấp chức năng gởi mail
    /// </summary>
    public partial class MailHeper
    {
        /// <summary>
        /// Gởi email
        /// </summary>
        /// <param name="mailServer">Địa chỉ mail server</param>
        /// <param name="port">Cổng mail server</param>
        /// <param name="enableSsl">Sử dụng SSL?</param>
        /// <param name="mailUser">Tài khoản đăng nhập mail</param>
        /// <param name="mailPassword">Mật khẩu đăng nhập của tài khoản email</param>
        /// <param name="emailFrom">Địa chỉ email gởi</param>
        /// <param name="emailTo">Địa chỉ email nhận</param>
        /// <param name="subject">Tiêu đề mail</param>
        /// <param name="body">Nội dung mail</param>
        /// <param name="isBodyHtml">Nội dung mail là HTML?</param>
        /// <returns></returns>
        public static void SendMail(string mailServer, int port, bool enableSsl,
                        string mailUser, string mailPassword,
                        string emailFrom, string emailTo,
                        string subject, string body, bool isBodyHtml)
        {
            using (MailMessage mailMessage = new MailMessage(emailFrom, emailTo))
            {
                SmtpClient mailClient = new SmtpClient(mailServer, port);
                mailClient.Timeout = 105000;
                mailClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                mailClient.Credentials = new NetworkCredential(mailUser, mailPassword);
                mailClient.EnableSsl = enableSsl;

                mailMessage.IsBodyHtml = isBodyHtml;
                mailMessage.SubjectEncoding = Encoding.UTF8;
                mailMessage.Subject = subject;
                mailMessage.Body = body;

                mailClient.Send(mailMessage);

            }
        }
        /// <summary>
        /// Gởi email
        /// </summary>
        /// <param name="mailServer">Địa chỉ mail server</param>
        /// <param name="port">Cổng mail server</param>
        /// <param name="enableSsl">Sử dụng SSL?</param>
        /// <param name="mailUser">Tài khoản đăng nhập mail</param>
        /// <param name="mailPassword">Mật khẩu đăng nhập của tài khoản email</param>
        /// <param name="emailFrom">Địa chỉ email gởi</param>
        /// <param name="emailTo">Địa chỉ email nhận</param>
        /// <param name="subject">Tiêu đề mail</param>
        /// <param name="body">Nội dung mail</param>
        /// <param name="isBodyHtml">Nội dung mail là HTML?</param>
        /// <returns></returns>
        public static async Task SendMailAsync(string mailServer, int port, bool enableSsl,
                        string mailUser, string mailPassword,
                        string emailFrom, string emailTo,
                        string subject, string body, bool isBodyHtml)
        {
            try
            {
                using (MailMessage mailMessage = new MailMessage(emailFrom, emailTo))
                {
                    SmtpClient mailClient = new SmtpClient(mailServer, port);
                    mailClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    mailClient.EnableSsl = true;
                    mailClient.UseDefaultCredentials = false;
                    mailClient.Credentials = new NetworkCredential(mailUser, mailPassword);
                    mailMessage.IsBodyHtml = isBodyHtml;
                    mailMessage.SubjectEncoding = Encoding.UTF8;
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;
                    await mailClient.SendMailAsync(mailMessage);
                }
            }
            catch
            {
            }
            
        }
        /// <summary>
        /// Địa chỉ mail hợp lệ?
        /// </summary>
        /// <param name="Email"></param>
        /// <returns></returns>
        private bool IsValidEmail(string Email)
        {
            return Regex.IsMatch(Email, @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
        }
    }
}
