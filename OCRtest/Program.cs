using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Security;
using System.Text.RegularExpressions;
using IronOcr;
using static OCRtest.AuthenticationManager;
using Microsoft.SharePoint.Client;
using System.Threading.Tasks;

namespace OCRtest
{
    class Program
    {
        static async Task Main()
        {
            ////se tienen que llenar con el usuario y la url del sitio
            //Uri site = new Uri("https://grupologisticoandreani.sharepoint.com/teams/TestPowerApps2");
            //string user = "evidela@contoso.onmicrosoft.com";
            //SecureString password = GetSecureString(user);



            //// Note: The PnP Sites Core AuthenticationManager class also supports this
            //using (var authenticationManager = new AuthenticationManager())
            //using (var context = authenticationManager.GetContext(site, user, password))
            //{
            //    context.Load(context.Web, p => p.Title);
            //    await context.ExecuteQueryAsync();
            //    Console.WriteLine($"Title: {context.Web.Title}");
            //}

            var tenantId = "2815c130-391b-4383-8743-e3413f343e08";
            var clientId = "df82286a-8297-412c-8fa0-a558fb62b826";
            var clientSecret = "tBtP-6avh3Xs1Z4U-m-nrJi-o4cdKO8~WB";
            var appScope = "api://df82286a-8297-412c-8fa0-a558fb62b826/Delegation";

            var userName = "evidela@andreani.com";
            var password = "";

            var siteUrl = new Uri("https://grupologisticoandreani.sharepoint.com/teams/TestPowerApps2");
           // var siteUrl = new Uri("https://grupologisticoandreani.sharepoint.com/teams/TestPowerApps2");
            using (var cc = new AuthenticationManager().GetContext(siteUrl, clientId, clientSecret, tenantId, appScope, userName, password))
            {
                Console.WriteLine("Using user on-behalf-of");
                cc.Load(cc.Web, p => p.Title);
                await cc.ExecuteQueryAsync();
                Console.WriteLine(cc.Web.Title);
            }
        }
        private static SecureString GetSecureString(string user)
        {
            // Instantiate the secure string.
            SecureString securePwd = new SecureString();
            ConsoleKeyInfo key;

            Console.Write("Enter password for {0}: ",user);
            do
            {
                key = Console.ReadKey(true);
                // Append the character to the password.
                if (key.Key != ConsoleKey.Enter)
                {
                    securePwd.AppendChar(key.KeyChar);
                    Console.Write("*");
                }
                // Exit if Enter key is pressed.
            } while (key.Key != ConsoleKey.Enter);
            return securePwd;
        }
    }
}
