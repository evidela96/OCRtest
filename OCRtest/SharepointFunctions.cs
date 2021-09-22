using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;

namespace OCRtest
{
    class SharepointFunctions
    {
        private static readonly string fotosConUbicacionFarmanet = "ImagenesDroneConUbicacion - Farmanet";
        private static readonly string fotosSinUbicacionFarmanet = "ImagenesDroneSinUbicacion - Farmanet";
        private static readonly string fotosConUbicacionRofina = "ImagenesDroneConUbicacion - Rofina";
        private static readonly string fotosSinUbicacionRofina = "ImagenesDroneSinUbicacion - Rofina";
        private static readonly string fotosConUbicacionBenaN2 = "ImagenesDroneConUbicacion - Benavidez Nave 2";
        private static readonly string fotosSinUbicacionBenaN2 = "ImagenesDroneSinUbicacion - Benavidez Nave 2";
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
        public static void ManageLibraries(ref string liberiaConUbicacionDestino, ref string libreriaSinUbicacionDestino)
        {
            ConsoleKeyInfo option, confirmation;
            do
            {
                Console.WriteLine("\nSelecciona una opcion (del 1 al 3) :");
                Console.WriteLine("\t1 - Farmanet");
                Console.WriteLine("\t2 - Rofina");
                Console.WriteLine("\t3 - Benavidez Nave 2");
                option = Console.ReadKey();

                Console.WriteLine("\nDesea Continuar ? (Ingrese 's' o 'n')");
                confirmation = Console.ReadKey();
                Console.WriteLine();
                
            } while (!(option.KeyChar >= '1' && option.KeyChar <= '3') || (confirmation.KeyChar == 'n'));

            switch (option.KeyChar)
            {
                case '1':
                    liberiaConUbicacionDestino = fotosConUbicacionFarmanet;
                    libreriaSinUbicacionDestino = fotosSinUbicacionFarmanet;
                    break;
                case '2':
                    liberiaConUbicacionDestino = fotosConUbicacionRofina;
                    libreriaSinUbicacionDestino = fotosSinUbicacionRofina;
                    break;
                case '3':
                    liberiaConUbicacionDestino = fotosConUbicacionBenaN2;
                    libreriaSinUbicacionDestino = fotosSinUbicacionBenaN2;
                    break;
            }
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
