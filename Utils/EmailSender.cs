namespace dotnershop.Utils
{
    using System.Threading.Tasks;
    using dotnetshop.Utils;

    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Task.CompletedTask;
        }
    }
}