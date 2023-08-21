using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WinDir.DesktopClient.Helpers
{
    public class MsalAuthHelper
    {

        private IPublicClientApplication _app;

        private static readonly string ClientId = "543d6e7c-2418-49bb-9e76-c3cc3013983c";

        //https://github.com/Azure-Samples/active-directory-b2c-dotnet-desktop/blob/msalv3/active-directory-b2c-wpf/App.xaml.cs
        private static readonly string TenantName = "MarconiStack";
        private static readonly string Tenant = $"{TenantName}.onmicrosoft.com";
        private static readonly string AzureAdB2CHostname = $"{TenantName}.b2clogin.com";

        public static string PolicySignUpSignIn = "B2C_1_SignUpAndSignIn";

        private static string AuthorityBase = $"https://{AzureAdB2CHostname}/tfp/{Tenant}/";
        public static string AuthoritySignUpSignIn = $"{AuthorityBase}{PolicySignUpSignIn}";

        private static readonly string RedirectUri = $"https://{TenantName}.b2clogin.com/oauth2/nativeclient";

        private static readonly string SpListScope = $"https://{Tenant}/75a3bd6b-14f0-4fe5-970a-9d3fb15991a7/queue.manage";

        private static readonly string[] Scopes = { SpListScope };

        private IList<IAccount> accounts;
        public AuthenticationResult AuthenticationResult;

        public IAccount ActiveAccount
        {
            get
            {
                return accounts.FirstOrDefault();
            }
        }

        public async Task<string> GetTokenAsync()
        {
            var aAccount = accounts.FirstOrDefault();
            if (aAccount == null)
            {
                return "";
            }
            else
            {
                AuthenticationResult = await _app.AcquireTokenSilent(Scopes, aAccount)
                    .ExecuteAsync()
                    .ConfigureAwait(false);
                return AuthenticationResult.AccessToken;
            }
        }

        public MsalAuthHelper()
        {
            //ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            _app = PublicClientApplicationBuilder.Create(ClientId)
                .WithB2CAuthority(AuthoritySignUpSignIn)
                .WithRedirectUri("http://localhost")
                .Build();
            TokenCacheHelper.EnableSerialization(_app.UserTokenCache);
            accounts = _app.GetAccountsAsync().Result.ToList();
        }

        public async Task SignInAsync()
        {
            accounts = (await _app.GetAccountsAsync()).ToList();
            AuthenticationResult = null;
            try
            {
                AuthenticationResult = await _app.AcquireTokenInteractive(Scopes)
                    .WithUseEmbeddedWebView(true)
                    //.WithParentActivityOrWindow(new WindowInteropHelper(this).Handle)
                    .WithAccount(accounts.FirstOrDefault())
                    //.WithPrompt(Prompt.SelectAccount)
                    .ExecuteAsync();

                accounts = (await _app.GetAccountsAsync()).ToList();

            }
            catch (MsalUiRequiredException)
            {

            }
            catch (MsalException ex)
            {
                // An unexpected error occurred.
                string message = ex.Message;
                if (ex.InnerException != null)
                {
                    message += "Error Code: " + ex.ErrorCode + "Inner Exception : " + ex.InnerException.Message;
                }
                MessageBox.Show(message);
            }


        }

        public async Task SignOutAsync()
        {
            // clear the cache
            while (accounts.Any())
            {
                await _app.RemoveAsync(accounts.First());
                accounts = (await _app.GetAccountsAsync()).ToList();
            }
            AuthenticationResult = null;
        }

        public bool IsSignedIn()
        {
            if (accounts.Any())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal async Task<object> GetNameOfActiveAccountAsync()
        {
            //using System.IdentityModel.Tokens.Jwt;
            var handler = new JwtSecurityTokenHandler();
            var tokenContent = (JwtSecurityToken)handler.ReadToken(await GetTokenAsync());
            var email = tokenContent.Claims.FirstOrDefault(x => x.Type == "emails")?.Value;
            var name = tokenContent.Claims.FirstOrDefault(x => x.Type == "name")?.Value;
            return name;
        }
    }
}
