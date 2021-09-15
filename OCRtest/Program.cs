﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Security;
using System.Text.RegularExpressions;
using IronOcr;
using static OCRtest.ImageFunctions;
using static OCRtest.SharepointFunctions;


namespace OCRtest
{
    class Program
    {
        static void Main()
        {
            var Ocr = new IronTesseract();
            bool hit;
            string finalImagePath = "C:/Users/Public/ControInventarioDrone/final_images/";
            string failImagePath = "C:/Users/Public/ControInventarioDrone/fail_images/";
            string cutImagesPath = "C:/Users/Public/ControInventarioDrone/cut_images/";
            string regEx = "([0-9]+-[0-9]+-[0-9]+)|([C,c,P,p][0-9]+-[0-9]+-[0-9]+)";

            string sourceFiles = "D:/DCIM/100MEDIA";
            Uri site = new Uri("https://grupologisticoandreani.sharepoint.com/teams/InventarioWH");
            string libreriaFotosConUbicacion = "ImagenesDron - Test";
            string libreriaFotosSinUbicacion = "ImagenesDronNoLeida - Test";
        string[] imagePathArray = Directory.GetFiles(sourceFiles);

            ManageDirectory(finalImagePath);
            ManageDirectory(failImagePath);
            ManageDirectory(cutImagesPath);

            foreach (var imagePath in imagePathArray)
            {
                hit = false;

                Console.WriteLine("Trying {0} ...", Path.GetFileName(imagePath));

                Bitmap b = new Bitmap(imagePath);
                Rectangle r = new Rectangle(0, 2176, 3968, 800);
                Bitmap croppedImage = CropImage(b, r);
                string cutPath = cutImagesPath + "cut_" + Path.GetFileNameWithoutExtension(imagePath) + ".png";
                croppedImage.Save(cutPath);

                using (var input = new OcrInput(cutPath))
                {

                    OcrResult result = Ocr.Read(input);
                    foreach (var line in result.Lines)
                    {
                        MatchCollection mc = Regex.Matches(line.Text, regEx);
                        foreach (Match m in mc)
                        {
                            if (m.Success)
                            {
                                hit = SaveImageWithLocation(imagePath, finalImagePath, m);
                            }
                        }
                        if (hit) break;
                    }
                    if (!hit)
                    {
                        foreach (var line in result.Lines)
                        {
                            if (Regex.Match(line.Text, "[0-9]").Success)
                            {
                                string linePath =
                                        cutImagesPath
                                        + "line_"
                                        + line.LineNumber
                                        + "_"
                                        + Path.GetFileNameWithoutExtension(imagePath)
                                        + ".png";
                                line.ToBitmap(input).Save(linePath);
                                MatchCollection mc = Regex.Matches(Ocr.Read(linePath).Text, regEx);

                                foreach (Match m in mc)
                                {
                                    if (m.Success)
                                    {

                                        hit = SaveImageWithLocation(imagePath, finalImagePath, m);
                                    }

                                }
                                if (hit) break;
                            }
                        }
                        if (!hit)
                        {
                            Console.WriteLine("MISS");
                            SaveImageWithNoLocation(imagePath, failImagePath);
                        }

                    }
                    
                }
            }


            ManageDirectory(cutImagesPath);

            Console.WriteLine("Usuario de Microsoft Office: ");
            string user = Console.ReadLine();
            SecureString password = GetSecureString(user);
            
            using (var authenticationManager = new AuthenticationManager())
            using (var context = authenticationManager.GetContext(site, user, password))
            {
                Console.WriteLine("Subiendo fotos con ubicacion a Sharepoint ...");
                foreach (var imagePath in Directory.GetFiles(finalImagePath))
                {
                    Console.WriteLine("\tSubiendo {0} ...", Path.GetFileName(imagePath));
                    UploadDocumentContentStream(context, libreriaFotosConUbicacion, imagePath);
                }

                Console.WriteLine("Subiendo fotos sin ubicacion a Sharepoint ...");
                foreach (var imagePath in Directory.GetFiles(failImagePath))
                {
                    Console.WriteLine("\tSubiendo {0} ...", Path.GetFileName(imagePath));
                    UploadDocumentContentStream(context, libreriaFotosSinUbicacion, imagePath);
                }
            }

        }
        
    }
}
