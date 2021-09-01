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
            Uri site = new Uri("https://grupologisticoandreani.sharepoint.com/teams/Inventario");
            string user = "evidela@andreani.com";
            SecureString password = GetSecureString();

            // Note: The PnP Sites Core AuthenticationManager class also supports this
            using (var authenticationManager = new AuthenticationManager())
            using (var context = authenticationManager.GetContext(site, user, password))
            {
                context.Load(context.Web, p => p.Title);
                await context.ExecuteQueryAsync();
                Console.WriteLine($"Title: {context.Web.Title}");
            }
        }
        private static SecureString GetSecureString()
        {
            // Instantiate the secure string.
            SecureString securePwd = new SecureString();
            ConsoleKeyInfo key;

            Console.Write("Enter password: ");
            do
            {
                key = Console.ReadKey(true);
                // Append the character to the password.
                if(key.Key != ConsoleKey.Enter) {
                    securePwd.AppendChar(key.KeyChar);
                    Console.Write("*");
                }
                // Exit if Enter key is pressed.
            } while (key.Key != ConsoleKey.Enter);
            return securePwd;
        }
    }
}
