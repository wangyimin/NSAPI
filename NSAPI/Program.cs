using System.Configuration;
using System.Threading.Tasks;

namespace NSAPI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new HttpNtlm().HttpOverNtlm(ConfigurationManager.AppSettings["NTLM_USER"],
                ConfigurationManager.AppSettings["NTLM_PASS"], ConfigurationManager.AppSettings["NTLM_DOMAIN"]);
        }

    }
}
