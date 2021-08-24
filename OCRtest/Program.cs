using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text.RegularExpressions;
using IronOcr;


namespace OCRtest
{
    class Program
    {
        private static readonly string finalImagePath= "C:/Users/Public/Picturesimages_3/final_images/";
        private static readonly string failImagePath = "C:/Users/Public/Picturesimages_3/fail_images/";
        private static readonly string cutImagesPath = "C:/Users/Public/Picturesimages_3/cut_images/";
        private static readonly String regEx = "[0-9]+-[0-9]+-[0-9]+";
        
        //se debe cambiar este directorio
        private static readonly string sourceFiles = "C:/Users/Exequiel/Desktop/images";

        static void Main(string[] args)
        {
            var Ocr = new IronTesseract();
            bool hit;
            string[] imagePathArray = Directory.GetFiles(sourceFiles);
            ManageDirectories();

            foreach (var imagePath in imagePathArray)
            {
                hit = false;
                Console.WriteLine("Trying {0} ...", Path.GetFileName(imagePath));
                
                Bitmap b = new Bitmap(imagePath);
                Rectangle r = new Rectangle(0, 100, 3968, 800);
                Bitmap croppedImage = cropImage(b, r);
                croppedImage.Save(cutImagesPath + "cut_" + Path.GetFileNameWithoutExtension(imagePath) + ".png");

                using (var input = new OcrInput(cutImagesPath + "cut_" + Path.GetFileNameWithoutExtension(imagePath) + ".png"))
                {
                    
                    OcrResult result = Ocr.Read(input);
                    foreach (var line in result.Lines)
                    {
                        Match m = Regex.Match(line.Text, regEx);
                        if(m.Success)
                        {

                            hit = saveImageWithLocation(imagePath,m);
                        }
                        if (hit) break;
                    }
                    if (!hit)
                    {
                        Console.WriteLine("MISS");
                        saveImageWithNoLocation(imagePath);
                    }
                }
                //Console.WriteLine("File {0} , Text read: {1}", Path.GetFileName(imagePath), Ocr.Read(
                //        imagePath,
                //        new Rectangle(0,0,3968,1300)
                //    ).Text);
                //foreach (var page in result.Pages)
                //{

                //MatchCollection mc = Regex.Matches(result.Text, regEx);

                //foreach (Match m in mc) {

                //    if (m.Success)
                //    {
                //        Console.WriteLine("File: {1} Match : {0}", m.Value, Path.GetFileName(imagePath));
                //        var name = m.Value;
                //        Image img = Image.FromFile(imagePath);
                //        img.Save(finalImagePath + m.Value +"_"+ Guid.NewGuid().ToString().Substring(0,3)+ ".jpg");
                //        hit = true;
                //    }
                //    if (hit) break;
                //}
                //if (!hit)
                //{
                //    Console.WriteLine("Could not read location of {0}", Path.GetFileName(imagePath));
                //    saveImageWithNoLocation(imagePath);
                //}
                //}
                //}
            }
        }

        private static void ManageDirectories()
        {
            if (!Directory.Exists(finalImagePath))
            {
                Directory.CreateDirectory(finalImagePath);
            }
            else
            {
                foreach (var filePath in Directory.GetFiles(finalImagePath))
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
        }
        private static void saveImageWithNoLocation(string imagePath)
        {
            Image m = Image.FromFile(imagePath);
            m.Save(failImagePath + Path.GetFileNameWithoutExtension(imagePath) + "_" + Guid.NewGuid().ToString().Substring(0, 4) + ".png");
        }
        private static bool saveImageWithLocation(string imagePath, Match m)
        {
            bool hit = true;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("File: {1} Match : {0}", m.Value, Path.GetFileName(imagePath));
            Console.ResetColor();
            var name = m.Value;
            Image img = Image.FromFile(imagePath);
            img.Save(finalImagePath + m.Value + "_" + Guid.NewGuid().ToString().Substring(0, 4) + ".png");
            return hit;
        }
        public static Bitmap cropImage(Bitmap source, Rectangle section)
        {
            var bitmap = new Bitmap(section.Width, section.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);
                return bitmap;
            }
        }
    }
}
