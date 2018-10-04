namespace Framework.Core.Interfaces.Utils
{
    public interface IConfigurationService
    {
        //string SessionTime();

        //string IdleTime();

        string EmailUserName();

        string GetPassword();

        string GetFromAddress();

        string GetToAddress();

        string GetSMTP();

        string HostAddress();

        string GetBlobContainer();
    }
}
