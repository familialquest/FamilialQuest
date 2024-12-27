using CommonLib;

namespace MailService.Services
{
    public interface IMailServices
    {
        void SendMessage(FQRequestInfo ri);
    }
}
