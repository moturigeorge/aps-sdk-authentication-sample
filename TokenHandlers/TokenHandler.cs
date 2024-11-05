using aps_sdk_authentication_sample.Models;
using Autodesk.Authentication;
using Autodesk.Authentication.Model;

namespace aps_sdk_authentication_sample.TokenHandlers
{
    public class TokenHandler
    {
        private AuthenticationClient authenticationClient = new AuthenticationClient();
        private ForgeConfiguration forgeConfiguration = new ForgeConfiguration();
        public static string refreshToken;
        public TokenHandler(IConfiguration configuration)
        {
            // Bind the configuration section to the ForgeConfiguration class
            forgeConfiguration = configuration.Get<ForgeConfiguration>();

            // Check if the forgeConfiguration is null
            if (forgeConfiguration == null || forgeConfiguration.Forge == null)
            {
                throw new Exception("Failed to bind Forge configuration.");
            }

            // Check if ClientId and ClientSecret are set
            if (string.IsNullOrEmpty(forgeConfiguration.Forge.ClientId) || string.IsNullOrEmpty(forgeConfiguration.Forge.ClientSecret))
            {
                throw new Exception("ClientId or ClientSecret is not set.");
            }
        }
        public ThreeLeggedToken Login()
        {
            try
            {
                ThreeLeggedToken token = new ThreeLeggedToken();
                var oAuthHandler = OAuthHandler.Create(forgeConfiguration.Forge);

                //We want to sleep the thread until we get 3L access_token.
                //https://stackoverflow.com/questions/6306168/how-to-sleep-a-thread-until-callback-for-asynchronous-function-is-received
                AutoResetEvent stopWaitHandle = new AutoResetEvent(false);

                oAuthHandler.Invoke3LeggedOAuth(async (bearer) =>
                {
                    // This is our application delegate. It is called upon success or failure
                    // after the process completed
                    if (bearer == null)
                    {
                        Console.WriteLine("Login Response", "Sorry, Authentication failed! 3legged test");
                        return;
                    }

                    token = bearer;
                    refreshToken = bearer.RefreshToken;
                    // The call returned successfully and you got a valid access_token.                
                    DateTime dt = DateTime.Now;
                    dt.AddSeconds(double.Parse(bearer.ExpiresIn.ToString()));
                    UserInfo profileApi = await authenticationClient.GetUserInfoAsync(bearer.AccessToken);
                    Console.WriteLine("Login Response", $"Hello {profileApi.Name} !!, You are Logged in!");
                    stopWaitHandle.Set();
                });
                stopWaitHandle.WaitOne();

                return token;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw new Exception(ex.Message.ToString());
            }
        }

        public async Task<string> Get2LeggedTokenAsync()
        {
            // Get 2Legged token.
            // Pass the  client Id and secret as in your app. The method
            try
            {
                TwoLeggedToken twoLeggedToken = await authenticationClient.GetTwoLeggedTokenAsync(
                    forgeConfiguration.Forge.ClientId,
                    forgeConfiguration.Forge.ClientSecret,
                    new List<Scopes>() { Scopes.DataRead, Scopes.BucketRead }
                    );
                string accessToken = twoLeggedToken.AccessToken;
                return accessToken;
            }
            catch (AuthenticationApiException ex)
            {
                Console.Write(ex.Message);
                return null;
            }
        }

        public async Task RefreshTokenAsync()
        {
            // Get Refresh token
            try
            {
                ThreeLeggedToken newToken = await authenticationClient.RefreshTokenAsync(refreshToken, forgeConfiguration.Forge.ClientId, clientSecret: forgeConfiguration.Forge.ClientSecret);
                string accessToken = newToken.AccessToken;
            }
            catch (AuthenticationApiException ex)
            {
                Console.Write(ex.Message);
            }
        }

    }
}