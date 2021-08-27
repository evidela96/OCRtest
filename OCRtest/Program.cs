using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Security;
using System.Text.RegularExpressions;
using IronOcr;
using static OCRtest.AuthenticationManager;

namespace OCRtest
{
    class Program
    {
        private static readonly string finalImagePath= "C:/Users/Public/Picturesimages_3/final_images/";
        public static string[] imagePathArray = Directory.GetFiles(finalImagePath);
        static async System.Threading.Tasks.Task Main()
        {
            Uri site = new Uri("https://grupologisticoandreani.sharepoint.com/teams/ControldeInventarioporDrone");
            string user = "evidela@andreani.com";
            Console.WriteLine("User : {0}",user);
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
