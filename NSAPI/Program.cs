using System.Configuration;

namespace NSAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            new HttpNtlm().HttpOverNtlm(ConfigurationManager.AppSettings["NTLM_USER"],
                ConfigurationManager.AppSettings["NTLM_PASS"], ConfigurationManager.AppSettings["NTLM_DOMAIN"]).Wait();
        }

    }
}
