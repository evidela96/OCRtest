using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using IronOcr;

namespace OCRtest
{
    class Program
    {
        private static readonly string cutImagesPath= "C:/Users/Public/Picturesimages/cut_images/";
        private static readonly string finalImagePath= "C:/Users/Public/Picturesimages/final_images/";
        private static readonly String regEx = "[0-9]+-[0-9]+-[0-9]+";

        //se debe cambiar este directorio
        private static readonly string sourceFiles = "C:/Users/evidela/OneDrive - ANDREANI LOGISTICA SA/Escritorio/images";

        static void Main(string[] args)
        {
            var Ocr = new IronTesseract();
            Ocr.Configuration.ReadBarCodes = false;

            string[] imagePathArray = Directory.GetFiles(sourceFiles);

            ManageDirectories();

            foreach (var imagePath in imagePathArray)
            {
                int imageWidth = Image.FromFile(imagePath).Width;
                using (var Input = new OcrInput(imagePath))
                {
                    
                    var ContentArea = new Rectangle() { X = 0, Y = 0, Height = 500, Width = imageWidth };
                    Input.AddImage(imagePath, ContentArea);

                    OcrResult result = Ocr.Read(Input);

                    foreach (var page in result.Pages)
                    {
                        foreach (var paragraph in page.Paragraphs)
                        {
                            foreach (var line in paragraph.Lines)
                            {
                                foreach (var word in line.Words)
                                {
                                    string wordPath =
                                        cutImagesPath
                                        + Path.GetFileNameWithoutExtension(imagePath)
                                        + "_cut_"
                                        + page.PageNumber
                                        + paragraph.ParagraphNumber
                                        + line.LineNumber
                                        + word.WordNumber + ".png";
                                    word.ToBitmap(Input).Save(wordPath);
                                    Match m = Regex.Match(Ocr.Read(wordPath).Text, regEx);

                                    if (m.Success)
                                    {
                                        Console.WriteLine(
                                            "HIT -> "+Path.GetFileName(imagePath)
                                            + ": Word Path : "
                                            + wordPath
                                            + " , Word Text : "
                                            + Ocr.Read(wordPath).Text
                                         );
                                        var name = m.Value;
                                        Image img = Image.FromFile(imagePath);
                                        img.Save(finalImagePath + m.Value + ".jpg");
                                        break;
                                    }
                                    else
                                    {
                                        Console.WriteLine(
                                            "MISS -> "+Path.GetFileName(imagePath)
                                            + ": Word Path : "
                                            + wordPath
                                            + " , Word Text : "
                                            + Ocr.Read(wordPath).Text
                                        );
                                    }
                                }
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
            if (!Directory.Exists(cutImagesPath))
            {
                Directory.CreateDirectory(cutImagesPath);
            }
            
        }
    }
}
