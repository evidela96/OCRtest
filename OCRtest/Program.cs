using Microsoft.SharePoint.Client;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace OCRtest
{
    class Program
    {
        static async Task Main()
        {
            Uri site = new Uri("https://grupologisticoandreani.sharepoint.com/teams/ControldeInventarioporDrone");
            string sourceFiles = "C:/Users/evidela/OneDrive - ANDREANI LOGISTICA SA/Escritorio/FotosPorDrone/final_images/003-045-30_fc9b.PNG";
            string libraryName = "FotosPorDroneBiblioteca";
            string user = "evidela@andreani.com";
            SecureString password = GetSecureString(user);

            using (var authenticationManager = new AuthenticationManager())
            using (var context = authenticationManager.GetContext(site, user, password))
            {
                Console.WriteLine("Uploading {0} to Sharepoint...", Path.GetFileName(sourceFiles));
                UploadDocumentContentStream(context, libraryName, sourceFiles);
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
        private static void InserItemToDocuments(ClientContext context ,string libraryName) 
        {
            string filePath = "C:/Users/evidela/OneDrive - ANDREANI LOGISTICA SA/Escritorio/FotosPorDrone/final_images/003-037-30_614c.PNG";

            FileCreationInformation fci = new FileCreationInformation();
            fci.Content = System.IO.File.ReadAllBytes(filePath);
            fci.Url = Path.GetFileName(filePath);
            Web web = context.Web;
            List targetDocLib = web.Lists.GetByTitle(libraryName);
            context.ExecuteQuery();
            Microsoft.SharePoint.Client.File newFile = targetDocLib.RootFolder.Files.Add(fci);
            context.Load(newFile);
            context.ExecuteQuery();
        }

        private static void InsertTextItem(ClientContext context,  string listName) {
            // Assume that the web has a list named "Announcements".
            List announcementsList = context.Web.Lists.GetByTitle(listName);

            // We are just creating a regular list item, so we don't need to
            // set any properties. If we wanted to create a new folder, for
            // example, we would have to set properties such as
            // UnderlyingObjectType to FileSystemObjectType.Folder.
            ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
            ListItem newItem = announcementsList.AddItem(itemCreateInfo);
            newItem["Title"] = "7";
           
           // newItem["Body"] = "Hello World!";
            newItem.Update();

            context.ExecuteQuery();
        }

        private static void InsertDocVideo(ClientContext context, string listName)
        {
            string filePath = "C:/Users/evidela/OneDrive - ANDREANI LOGISTICA SA/Imágenes/Capturas de pantalla/reporteDuracell.PNG";

            FileCreationInformation fcinfo = new FileCreationInformation();
            fcinfo.Content = System.IO.File.ReadAllBytes(filePath);
            fcinfo.Url =Path.GetFileName(filePath);
            fcinfo.Overwrite = true;

            Web myWeb = context.Web;
            List myLibrary = myWeb.Lists.GetByTitle(listName);
            myLibrary.RootFolder.Files.Add(fcinfo);

            context.ExecuteQuery();

        }

        //public static void SaveBinaryDirect(ClientContext ctx, string libraryName, string filePath)
        //{
        //    Web web = ctx.Web;
        //    // Ensure that the target library exists. Create it if it is missing.
        //    //if (!LibraryExists(ctx, web, libraryName))
        //    //{
        //    //    CreateLibrary(ctx, web, libraryName);
        //    //}

        //    using (FileStream fs = new FileStream(filePath, FileMode.Open))
        //    {
        //        Microsoft.SharePoint.Client.File.SaveBinaryDirect(ctx, string.Format("/{0}/{1}", libraryName, Path.GetFileName(filePath)), fs, true);
        //    }
        //}
        public static void UploadDocumentContentStream(ClientContext ctx, string libraryName, string filePath)
        {
            Web web = ctx.Web;

            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                FileCreationInformation flciNewFile = new FileCreationInformation();

                // This is the key difference for the first case - using ContentStream property
                flciNewFile.ContentStream = fs;
                flciNewFile.Url = Path.GetFileName(filePath);
                flciNewFile.Overwrite = true;

                List docs = web.Lists.GetByTitle(libraryName);
                
                Microsoft.SharePoint.Client.File uploadFile = docs.RootFolder.Files.Add(flciNewFile);
                
                ctx.Load(uploadFile);
                ctx.ExecuteQuery();
            }
        }

        public static void insertURL(ClientContext ctx , string siteUrl , string libraryName , string filePath) {
            string absoluteURL = siteUrl + "/" + libraryName + "/" + Path.GetFileName(filePath);

            List oList = ctx.Web.Lists.GetByTitle(libraryName);
            //ListItem oListItem = oList.GetItemById(Path.GetFileName(filePath));
            ctx.Load(oList);
            ctx.ExecuteQuery();

            CamlQuery camlQuery = new CamlQuery();
            camlQuery.ViewXml =
               @"<View>  
               <Query> 
                  <Where><Eq><FieldRef Name='FileLeafRef' /><Value Type='File'>" + Path.GetFileName(filePath) + @"</Value></Eq></Where> 
               </Query> 
                <ViewFields><FieldRef Name='FileRef' /><FieldRef Name='FileLeafRef' /></ViewFields> 
         </View>";

            ListItemCollection listItems = oList.GetItems(camlQuery);
            ctx.Load(listItems);
            ctx.ExecuteQuery();
            var listItem = listItems.FirstOrDefault();
            Console.WriteLine(listItem["FileRef"].ToString());
            //oListItem["URL"] = absoluteURL;
            //oListItem.Update();
            //ctx.ExecuteQuery();
        }
        public static void uploadFileAsAttach(ClientContext ctx) {
            var list = ctx.Web.Lists.GetByTitle("FotosPorDrone");
            ListItem listItem = list.GetItemById("7");
            var path = "C:/Users/evidela/OneDrive - ANDREANI LOGISTICA SA/Escritorio/FotosPorDrone/final_images/003-042-20_c06b.PNG";
            var attachmentInfo = new AttachmentCreationInformation();
            attachmentInfo.FileName = Path.GetFileName(path);
            
            using (var fs = new FileStream(path, FileMode.Open))
            {
                attachmentInfo.ContentStream = fs;
                var attachment = listItem.AttachmentFiles.Add(attachmentInfo);
                ctx.ExecuteQuery();
            }
        }
    }
}
