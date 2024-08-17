using NSspi;
using NSspi.Contexts;
using NSspi.Credentials;
using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace NSAPI
{
    internal class HttpNtlm
    {
        private static readonly String URI = ConfigurationManager.AppSettings["URI"];
        
        public async Task HttpOverNtlm(String user, String password, String domain)
        {
            byte[] clientToken = default;

            using (PasswordCredential clientCred = new PasswordCredential(
                        domain, user, password, PackageNames.Ntlm, CredentialUse.Outbound))
            using (ClientContext client = new ClientContext(
                        clientCred, clientCred.PrincipleName, ContextAttrib.Zero))
            { 
                Console.WriteLine(
                    await GetContentsAsync((phase, req, auth) =>
                        {
                            if (phase == 1)
                            {
                                if (!auth.Any(el => el.Scheme.Equals("NTLM")))
                                    throw new InvalidOperationException("Only WwwAuthenticate NTLM is supported,");

                                client.Init(null, out clientToken);
                                req.Add("Authorization", "NTLM " + Convert.ToBase64String(clientToken));
                            }
                            else if (phase == 2)
                            {
                                client.Init(Convert.FromBase64String(auth.FirstOrDefault()?.Parameter), out clientToken);
                                req.Add("Authorization", "NTLM " + Convert.ToBase64String(clientToken));
                            }
                        }));
            }

        }

        private async Task<String> GetContentsAsync(
            Action<int, HttpRequestHeaders, HttpHeaderValueCollection<AuthenticationHeaderValue>> action)
        {
            HttpRequestMessage req = default;
            HttpResponseMessage resp = default;
            HttpHeaderValueCollection<AuthenticationHeaderValue> auth = default;

            int phase = 0;

            using (HttpClient hc = new HttpClient())
            {
                try
                {
                    while (phase < 3)
                    {
                        req = new HttpRequestMessage(HttpMethod.Get, URI);
                        action(phase, req.Headers, auth);

                        resp = await hc.SendAsync(req);
#if LOGGING
                        Console.WriteLine(resp);
#endif

                        if (resp.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            auth = resp.Headers.WwwAuthenticate;
                            phase++;
                        }
                        else if (resp.StatusCode == HttpStatusCode.OK)
                        {
                            return await resp.Content.ReadAsStringAsync();
                        }
                        else
                        {
                            throw new InvalidOperationException("Unknown error.");
                        }

                    }
                }
                finally
                {
                    req?.Dispose();
                    resp?.Dispose();
                }

                throw new InvalidOperationException("Logon failed.");
            }
        }
    }
}
