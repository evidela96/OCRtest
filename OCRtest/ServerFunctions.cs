using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace OCRtest
{
    class ServerFunctions
    {
        public static SecureString GetSecureString(string user)
        {
            // Instantiate the secure string.
            SecureString securePwd = new SecureString();
            ConsoleKeyInfo key;

            Console.Write("Contraseña para {0}: ", user);
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
        public static void ManageFileServerFolders(string cliente,ref string liberiaConUbicacionDestino, ref string libreriaSinUbicacionDestino, ref string rutaFotosOriginales , ref string rutaCut_Images, ref IConfigurationRoot configuration )
        {
            liberiaConUbicacionDestino = configuration["Destinos:"+cliente+":ConUbicacion"];
            libreriaSinUbicacionDestino = configuration["Destinos:"+cliente+":SinUbicacion"];
            rutaFotosOriginales = configuration["Destinos:"+cliente+":FotosOriginales"];
            rutaCut_Images = configuration["Destinos:"+cliente+":cut_images"];
        }

        public static void UploadToSharepointLibraries(string finalImagePath, string failImagePath, Uri site, string liberiaConUbicacionDestino, string libreriaSinUbicacionDestino)
        {
            string user;
            SecureString password;
            ConsoleKeyInfo confir;
            do
            {
                Console.WriteLine("Usuario de Microsoft Office: ");
                user = Console.ReadLine();
                password = GetSecureString(user);

                Console.WriteLine("Desea continuar ? (ingrese 's' o 'n')");
                confir = Console.ReadKey();

            } while (confir.KeyChar != 's');

            using (var authenticationManager = new AuthenticationManager())
            using (var context = authenticationManager.GetContext(site, user, password))
            {
                Console.WriteLine("Subiendo fotos con ubicacion a Sharepoint ...");
                foreach (var imagePath in Directory.GetFiles(finalImagePath))
                {
                    Console.WriteLine("\tSubiendo {0} ...", Path.GetFileName(imagePath));
                    UploadDocumentContentStream(context, liberiaConUbicacionDestino, imagePath);
                }

                Console.WriteLine("Subiendo fotos sin ubicacion a Sharepoint ...");
                foreach (var imagePath in Directory.GetFiles(failImagePath))
                {
                    Console.WriteLine("\tSubiendo {0} ...", Path.GetFileName(imagePath));
                    UploadDocumentContentStream(context, libreriaSinUbicacionDestino, imagePath);
                }
            }
        }
        
    }
}
