using IronOcr;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using static OCRtest.ImageFunctions;
using static OCRtest.ServerFunctions;

namespace OCRtest
{
    class Program
    {
        static void Main()
        {
            bool hit;
            string regEx = "([0-9]+-[0-9]+-[2-9][0-9])|([C,c,P,p][0-9]+-[0-9]+-[2-9][0-9])";

            string sourceFiles = "";
            string cutImagesPath = "";
            string carpetaConUbicacion = "";
            string carpetaSinUbicacion = "";

            int hours, minutes, seconds , miliseconds,totalMiliseconds;

            for (;;)
            {
                var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                var configuration = builder.Build();
                Installation.LicenseKey = Encoding.UTF8.GetString(Convert.FromBase64String(configuration["LicenseKey"]));
                var Ocr = new IronTesseract();

                hours = int.Parse(configuration["TimeToSleep:Hours"]);
                minutes = int.Parse(configuration["TimeToSleep:Minutes"]);
                seconds = int.Parse(configuration["TimeToSleep:Seconds"]);
                miliseconds = int.Parse(configuration["TimeToSleep:Miliseconds"]);

                totalMiliseconds = miliseconds + 1000 * (seconds + 60 * minutes + 3600 * hours);

                string[] clientes = configuration["Clientes"].Split(",");

                

                foreach (var c in clientes)
                {
                    Console.WriteLine("PROCESANDO CLIENTE:{0}", c);
                    ManageFileServerFolders(c, ref carpetaConUbicacion, ref carpetaSinUbicacion, ref sourceFiles, ref cutImagesPath, ref configuration);

                    if (Directory.GetFiles(sourceFiles).Length > 0)
                    {
                        //string[] imagePathArray = Directory.GetFiles(sourceFiles, "*.jp*;*.png");

                        List<string> list = new List<string>();
                        list.AddRange(Directory.GetFiles(sourceFiles, "*.jp*"));
                        list.AddRange(Directory.GetFiles(sourceFiles, "*.png*"));

                        String[] imagePathArray = list.ToArray();

                        foreach (var imagePath in imagePathArray)
                        {
                            hit = false;

                            Console.WriteLine("Trying {0} ...", Path.GetFileName(imagePath));

                            Bitmap b = new Bitmap(imagePath);
                            Rectangle r = new Rectangle(0, 2176, 3968, 800);
                            Bitmap croppedImage = CropImage(b, r);
                            string cutPath = cutImagesPath + "cut_" + Path.GetFileNameWithoutExtension(imagePath) + ".png";
                            croppedImage.Save(cutPath);
                            b.Dispose();

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
                                            hit = SaveImageWithLocation(imagePath, carpetaConUbicacion, m);
                                            
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

                                                    hit = SaveImageWithLocation(imagePath, carpetaConUbicacion, m);
                                                    
                                                }

                                            }
                                            if (hit) break;
                                        }
                                    }
                                    if (!hit)
                                    {
                                        Console.WriteLine("MISS");
                                        SaveImageWithNoLocation(imagePath, carpetaSinUbicacion);
                                        
                                    }

                                }

                            }
                        }
                        ManageDirectory(cutImagesPath);
                        ManageDirectory(sourceFiles);

                    }
                }
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Sleeping for {0} Hour(s) {1} Minute(s) {2} Second(s) {3} Milisecond(s)", hours,minutes,seconds,miliseconds);
                Console.ResetColor();
                Thread.Sleep(totalMiliseconds);
            }
            
        }
    }
}
