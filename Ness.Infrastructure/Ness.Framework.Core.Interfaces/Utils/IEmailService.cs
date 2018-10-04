namespace Framework.Core.Interfaces.Utils
{
    public interface IEmailService
    {
        void Send(string fullName, string email, string subject, string body, string attachmentFileName, string displayName);
    }
}