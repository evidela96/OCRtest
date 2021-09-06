using Microsoft.SharePoint.Client;
using System;
using System.Text;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Security;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Net;
using Microsoft.Identity.Client;

namespace OCRtest
{
    class AuthenticationManager:IDisposable
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private const string tokenEndpoint = "https://login.microsoftonline.com/common/oauth2/token";

        //aca hay que modificar este id con el id que te genera Azure AD
        private const string defaultAADAppId = "df82286a-8297-412c-8fa0-a558fb62b826";

        // Token cache handling
        private static readonly SemaphoreSlim semaphoreSlimTokens = new SemaphoreSlim(1);
        private AutoResetEvent tokenResetEvent = null;
        private readonly ConcurrentDictionary<string, string> tokenCache = new ConcurrentDictionary<string, string>();
        private bool disposedValue;

        internal class TokenWaitInfo
        {
            public RegisteredWaitHandle Handle = null;
        }
        public ClientContext GetContext(Uri web, string clientId, string clientSecret, string tenantId, string appScope, string userName, string password)
        {
            var context = new ClientContext(web);
            var resourceUri = new Uri($"{web.Scheme}://{web.DnsSafeHost}");

            async Task<string> AcquireTokenAsyncFunc(string clientIdFunc, string tenantIdFunc) => await AcquireTokenAsync(resourceUri, clientIdFunc, clientSecret, tenantIdFunc, appScope, userName, password);
            context.ExecutingWebRequest += (sender, e) =>
            {
                var cacheKey = $"{web.DnsSafeHost}_{userName}";
                string accessToken = EnsureAccessTokenAsync(resourceUri, cacheKey, clientId, tenantId, AcquireTokenAsyncFunc).GetAwaiter().GetResult();
                e.WebRequestExecutor.RequestHeaders["Authorization"] = $"Bearer {accessToken}";
            };

            return context;
        }


        public async Task<string> EnsureAccessTokenAsync(Uri resourceUri, string cacheKey, string clientId, string tenantId, Func<string, string, Task<string>> acquireTokenAsyncFunc)
        {
            string accessTokenFromCache = TokenFromCache(cacheKey, tokenCache);
            if (accessTokenFromCache == null)
            {
                await semaphoreSlimTokens.WaitAsync().ConfigureAwait(false);
                try
                {
                    // No async methods are allowed in a lock section
                    string accessToken = await acquireTokenAsyncFunc(clientId,tenantId).ConfigureAwait(false);
                    Console.WriteLine($"Successfully requested new access token resource {resourceUri.DnsSafeHost} for user {clientId}");
                    AddTokenToCache(cacheKey, tokenCache, accessToken);

                    // Register a thread to invalidate the access token once's it's expired
                    tokenResetEvent = new AutoResetEvent(false);
                    TokenWaitInfo wi = new TokenWaitInfo();
                    wi.Handle = ThreadPool.RegisterWaitForSingleObject(
                        tokenResetEvent,
                        async (state, timedOut) =>
                        {
                            if (!timedOut)
                            {
                                TokenWaitInfo internalWaitToken = (TokenWaitInfo)state;
                                if (internalWaitToken.Handle != null)
                                {
                                    internalWaitToken.Handle.Unregister(null);
                                }
                            }
                            else
                            {
                                try
                                {
                                    // Take a lock to ensure no other threads are updating the SharePoint Access token at this time
                                    await semaphoreSlimTokens.WaitAsync().ConfigureAwait(false);
                                    RemoveTokenFromCache(cacheKey, tokenCache);
                                    Console.WriteLine($"Cached token for resource {resourceUri.DnsSafeHost} and user {clientId} expired");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Something went wrong during cache token invalidation: {ex.Message}");
                                    RemoveTokenFromCache(cacheKey, tokenCache);
                                }
                                finally
                                {
                                    semaphoreSlimTokens.Release();
                                }
                            }
                        },
                        wi,
                        (uint)CalculateThreadSleep(accessToken).TotalMilliseconds,
                        true
                    );

                    return accessToken;

                }
                finally
                {
                    semaphoreSlimTokens.Release();
                }
            }
            else
            {
                Console.WriteLine($"Returning token from cache for resource {resourceUri.DnsSafeHost} and user {clientId}");
                return accessTokenFromCache;
            }
        }
        private async Task<string> AcquireTokenAsync(Uri resourceUri, string clientId, string clientSecret, string tenantId, string appScope, string userName, string password)
        {
            string[] spScopes = new string[] { $"{resourceUri.Scheme}://{resourceUri.DnsSafeHost}/.default" };
            string[] appScopes = new string[] { appScope };

            var pubApp = PublicClientApplicationBuilder
                .Create(clientId)
                .WithTenantId(tenantId)
                .Build();
            var pubResult = await pubApp.AcquireTokenByUsernamePassword(appScopes, userName, new NetworkCredential(string.Empty, password).SecurePassword).ExecuteAsync();
            var userToken = pubResult.AccessToken;
            var userAssertion = new UserAssertion(userToken);

            var app = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithTenantId(tenantId)
                .WithClientSecret(clientSecret)
                .Build();

            var result = await app.AcquireTokenOnBehalfOf(spScopes, userAssertion).ExecuteAsync();
            return result.AccessToken;

        }
        private static string TokenFromCache(string cacheKey, ConcurrentDictionary<string, string> tokenCache)
            {
                if (tokenCache.TryGetValue(cacheKey, out string accessToken))
                {
                    return accessToken;
                }

                return null;
            }

        private static void AddTokenToCache(string cacheKey, ConcurrentDictionary<string, string> tokenCache, string newAccessToken)
        {
            if (tokenCache.TryGetValue(cacheKey, out string currentAccessToken))
            {
                tokenCache.TryUpdate(cacheKey, newAccessToken, currentAccessToken);
            }
            else
            {
                tokenCache.TryAdd(cacheKey, newAccessToken);
            }
        }

        private static void RemoveTokenFromCache(string cacheKey, ConcurrentDictionary<string, string> tokenCache)
        {
            tokenCache.TryRemove(cacheKey, out string currentAccessToken);
        }

        private static TimeSpan CalculateThreadSleep(string accessToken)
        {
            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(accessToken);
            var lease = GetAccessTokenLease(token.ValidTo);
            lease = TimeSpan.FromSeconds(lease.TotalSeconds - TimeSpan.FromMinutes(5).TotalSeconds > 0 ? lease.TotalSeconds - TimeSpan.FromMinutes(5).TotalSeconds : lease.TotalSeconds);
            return lease;
        }

        private static TimeSpan GetAccessTokenLease(DateTime expiresOn)
        {
            DateTime now = DateTime.UtcNow;
            DateTime expires = expiresOn.Kind == DateTimeKind.Utc ? expiresOn : TimeZoneInfo.ConvertTimeToUtc(expiresOn);
            TimeSpan lease = expires - now;
            return lease;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (tokenResetEvent != null)
                    {
                        tokenResetEvent.Set();
                        tokenResetEvent.Dispose();
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
