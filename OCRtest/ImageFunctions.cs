using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace OCRtest
{
    class ImageFunctions
    {
        public static void ManageDirectory(string path)
        {
            //GC.Collect();
            //GC.WaitForPendingFinalizers();
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                string[] files = Directory.GetFiles(path);
                foreach (var filePath in files)
                {
                   File.Delete(filePath);
                }
                
            }
            
        }
        public static void SaveImageWithNoLocation(string imagePath , string failImagePath)
        {
            var bytes = File.ReadAllBytes(imagePath);
            var ms = new MemoryStream(bytes);
            var img = Image.FromStream(ms);
            img.Save(failImagePath + Path.GetFileNameWithoutExtension(imagePath) + ".png");
            img.Dispose();
            //using (Image m = Image.FromFile(imagePath))
            //{
            //    m.Save(failImagePath + Path.GetFileNameWithoutExtension(imagePath) + ".png");
            //};

        }
        public static bool SaveImageWithLocation(string imagePath, string finalImagePath , Match m)
        {
            bool hit = true;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("File: {1} Match : {0}", m.Value, Path.GetFileName(imagePath));
            Console.ResetColor();

            var bytes = File.ReadAllBytes(imagePath);
            var ms = new MemoryStream(bytes);
            var img = Image.FromStream(ms);
            img.Save(finalImagePath + m.Value + ".png");
            img.Dispose();
            //using (Image img = Image.FromFile(imagePath))
            //{
            //    img.Save(finalImagePath + m.Value + ".png");
            //};
            return hit;
        }
        public static Bitmap CropImage(Bitmap source, Rectangle section)
        {
            var bitmap = new Bitmap(section.Width, section.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);
                return bitmap;
            };
        }

    }
}
