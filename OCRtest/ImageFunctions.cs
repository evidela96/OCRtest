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
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                foreach (var filePath in Directory.GetFiles(path))
                {
                    File.Delete(filePath);
                }
            }
            
        }
        public static void SaveImageWithNoLocation(string imagePath , string failImagePath)
        {
            Image m = Image.FromFile(imagePath);
            m.Save(failImagePath + Path.GetFileNameWithoutExtension(imagePath) + "_" + Guid.NewGuid().ToString().Substring(0, 4) + ".png");
        }
        public static bool SaveImageWithLocation(string imagePath, string finalImagePath , Match m)
        {
            bool hit = true;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("File: {1} Match : {0}", m.Value, Path.GetFileName(imagePath));
            Console.ResetColor();
            Image img = Image.FromFile(imagePath);
            img.Save(finalImagePath + m.Value + "_" + Guid.NewGuid().ToString().Substring(0, 4) + ".png");
            return hit;
        }
        public static Bitmap CropImage(Bitmap source, Rectangle section)
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
