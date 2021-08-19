using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using IronOcr;

namespace OCRtest
{
    class Program
    {
        private static readonly string cutImagesPath= "C:/Users/Public/Picturesimages_2/cut_images/";
        private static readonly string resizeCutImagesPath = "C:/Users/Public/Picturesimages_2/resize_cut_images/";
        private static readonly string finalImagePath= "C:/Users/Public/Picturesimages_2/final_images/";
        private static readonly string failImagePath = "C:/Users/Public/Picturesimages_2/fail_images/";
        private static readonly String regEx = "[0-9]+-[0-9]+-[0-9]+";
        private static readonly string regEx2 = "[0-9][0-9][0-9] [0-9][0-9][0-9] [0-9][0-9]";

        //se debe cambiar este directorio
        private static readonly string sourceFiles ="C:/Users/evidela/OneDrive - ANDREANI LOGISTICA SA/Escritorio/images";
        
        static void Main(string[] args)
        {
            var Ocr = new IronTesseract();
            Ocr.Configuration.ReadBarCodes = false;
            bool hit;
            
            string[] imagePathArray = Directory.GetFiles(sourceFiles);

            ManageDirectories();

            foreach (var imagePath in imagePathArray)
            {
                Console.ForegroundColor= ConsoleColor.Red;
                Console.WriteLine("Trying " + Path.GetFileName(imagePath) + " ...");
                Console.ResetColor();
                using (var input = new OcrInput(imagePath))
                {
                    
                    OcrResult result = Ocr.Read(input);
                    
                    foreach(var line in result.Lines)
                    {
                        if(Regex.Match(line.Text , "[0-9]").Success)
                        {

                            string linePath =
                                cutImagesPath
                                + Path.GetFileNameWithoutExtension(imagePath)
                                + "_Line_"
                                + line.LineNumber
                                + ".png";

                            line.ToBitmap(input).Save(linePath);
                            string resizeCutImagePath =
                                resizeCutImagesPath
                                + Path.GetFileNameWithoutExtension(imagePath)
                                + "_ResizeLine_"
                                + line.LineNumber
                                + ".png";
                            Resize(linePath, resizeCutImagePath, 3.0);
                            
                            if (Regex.Match(Ocr.Read(resizeCutImagePath).Text,regEx).Success) {
                                Console.WriteLine("File: " + Path.GetFileName(resizeCutImagePath) + " Read: " + Ocr.Read(resizeCutImagePath).Text);
                            }
                            
                        }
                    }
                }
            }
        }

        private static void ManageDirectories()
        {
            if (!Directory.Exists(finalImagePath)) {
                Directory.CreateDirectory(finalImagePath);
            }
            else
            {
                foreach(var filePath in Directory.GetFiles(finalImagePath))
                {
                    File.Delete(filePath);
                }
            }
            if (!Directory.Exists(cutImagesPath))
            {
                Directory.CreateDirectory(cutImagesPath);
            }
            else
            {
                foreach (var filePath in Directory.GetFiles(cutImagesPath))
                {
                    File.Delete(filePath);
                }
            }
            if (!Directory.Exists(failImagePath))
            {
                Directory.CreateDirectory(failImagePath);
            }
            else
            {
                foreach (var filePath in Directory.GetFiles(failImagePath))
                {
                    File.Delete(filePath);
                }
            }
            
            if (!Directory.Exists(resizeCutImagesPath))
            {
                Directory.CreateDirectory(resizeCutImagesPath);
            }
            else
            {
                foreach (var filePath in Directory.GetFiles(resizeCutImagesPath))
                {
                    File.Delete(filePath);
                }
            }
        }
        public static void Resize(string imageFile, string outputFile, double scaleFactor)
        {
            using (var srcImage = Image.FromFile(imageFile))
            {
                var newWidth = (int)(srcImage.Width * scaleFactor);
                var newHeight = (int)(srcImage.Height * scaleFactor);
                using (var newImage = new Bitmap(newWidth, newHeight))
                using (var graphics = Graphics.FromImage(newImage))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.DrawImage(srcImage, new Rectangle(0, 0, newWidth, newHeight));
                    newImage.Save(outputFile);
                }
            }
        }
    }
}
