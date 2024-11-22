using Grpc.Core;
using System.Net;
using System.Net.Mail;
namespace GrpcServer.Services
{
    public class Service2S : Service2.Service2Base
    {
        private readonly ILogger<GreeterService> _logger;
        public Service2S(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override async Task<ReplyService2> SayHelloFromService2(RequestService2 request, ServerCallContext context)
        {
            //return Task.FromResult(new ReplyService2
            //{
            //    Message = "Hello from GRPC service 2 " + request.Name
            //});
            string emailTo = request.Name;
            string subject = "Hello from GRPC Service 2";
            string body = "This is a test email sent from GRPC Service 2.";

            bool isEmailSent = await SendEmailAsync(emailTo, subject, body);

            return new ReplyService2
            {
                Message = isEmailSent
                    ? $"Email successfully sent to {emailTo}"
                    : $"Failed to send email to {emailTo}"
            };

            //return Task.FromResult(new ReplyService2
            //{
            //    Message = "Hello from GRPC service 2 " + request.Name
            //});
        }
        private async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtpClient.Credentials = new NetworkCredential("minhquangpham502@gmail.com", "sekr zssb poag qycf");
                    smtpClient.EnableSsl = true;                    

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("minhquangpham502@gmail.com"),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(toEmail);

                    await smtpClient.SendMailAsync(mailMessage);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending email.");
                return false;
            }
        }
        
    }
}