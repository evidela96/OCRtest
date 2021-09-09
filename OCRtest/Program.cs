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
            Uri site = new Uri("https://grupologisticoandreani.sharepoint.com/teams/ControldeInventarioporDrone");
            string listName = "FotosPorDrone";
            string user = "evidela@andreani.com";
            SecureString password = GetSecureString(user);

            using (var authenticationManager = new AuthenticationManager())
            using (var context = authenticationManager.GetContext(site, user, password))
            {
                Console.WriteLine("Write new Title item:");
                string textInput = Console.ReadLine();
                InsertItem(context,textInput,site.ToString(),listName);
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
            Console.WriteLine();
            return securePwd;
        }
        private static void InsertItem(ClientContext context , string textInput , string siteUri , string listName) 
        {
            // Assume that the web has a list named "Announcements".
            List announcementsList = context.Web.Lists.GetByTitle(listName);

            // We are just creating a regular list item, so we don't need to
            // set any properties. If we wanted to create a new folder, for
            // example, we would have to set properties such as
            // UnderlyingObjectType to FileSystemObjectType.Folder.
            ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
            ListItem newItem = announcementsList.AddItem(itemCreateInfo);

            FileCreationInformation newFile = new FileCreationInformation();
            newFile.Content = System.IO.File.ReadAllBytes("C:/Users/evidela/OneDrive - ANDREANI LOGISTICA SA/Escritorio/FotosPorDrone/final_images/003-037-30_614c.PNG");
            newFile.Url = "C:/Users/evidela/OneDrive - ANDREANI LOGISTICA SA/Escritorio/FotosPorDrone/final_images/003-037-30_614c.PNG";

            newItem["Title"] = newFile;
            //newItem["Body"] = body;
            newItem.Update();

            context.ExecuteQuery();
        }
    }
}
