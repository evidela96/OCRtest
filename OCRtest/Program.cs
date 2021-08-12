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
        private static readonly string sourceFiles = "C:/Users/evidela/OneDrive - ANDREANI LOGISTICA SA/Escritorio/capturasVideo";

        static void Main(string[] args)
        {
            var Ocr = new IronTesseract();
            //Ocr.Configuration.ReadBarCodes = false;

            string[] imagePathArray = Directory.GetFiles(sourceFiles);
            bool nextImage = false;

            //ManageDirectories();

            foreach (var imagePath in imagePathArray)
            {
                //nextImage = false;
                //int imageWidth = Image.FromFile(imagePath).Width;
                //int imageHeight = Image.FromFile(imagePath).Height;
                using (var Input = new OcrInput(imagePath))
                {
                    //Input.Binarize();
                    //var ContentArea = new Rectangle() { X = 0, Y = 0, Height = imageHeight, Width = imageWidth };
                    //Input.AddImage(imagePath, ContentArea);
                    //Console.WriteLine("FILE: " + Path.GetFileNameWithoutExtension(sourceFiles));
                    OcrResult result = Ocr.Read(Input);

                    foreach (var page in result.Pages)
                    {
                        //Console.WriteLine(" Page Number {0} , Text:{1}", page.PageNumber,page.Text);
                        
                        foreach (var paragraph in page.Paragraphs)
                        {
                            //Console.WriteLine("  Paragraph Number {0} , Text:{1}", paragraph.ParagraphNumber, paragraph.Text);
                            foreach (var line in paragraph.Lines)
                            {
                                //Console.WriteLine("   Line Number {0} , Text:{1}", line.LineNumber, line.Text);
                                foreach (var word in line.Words)
                                {
                                    //Console.WriteLine("    Word Number {0} , Text:{1}", word.WordNumber, word.Text);
                                    //string wordPath =
                                    //    cutImagesPath
                                    //    + Path.GetFileNameWithoutExtension(imagePath)
                                    //    + "_cut_"
                                    //    + page.PageNumber
                                    //    + paragraph.ParagraphNumber
                                    //    + line.LineNumber
                                    //    + word.WordNumber + ".png";
                                    //word.ToBitmap(Input).Save(wordPath);
                                    //Match m = Regex.Match(Ocr.Read(wordPath).Text, regEx);

                                    //if (m.Success)
                                    //{
                                    //    nextImage = true;
                                    //    Console.WriteLine(
                                    //        "HIT -> "+Path.GetFileName(imagePath)
                                    //        + ": Word Path : "
                                    //        + wordPath
                                    //        + " , Word Text : "
                                    //        + Ocr.Read(wordPath).Text
                                    //     );
                                    //    var name = m.Value;
                                    //    Image img = Image.FromFile(imagePath);
                                    //    img.Save(finalImagePath + m.Value + ".jpg");
                                    //    break;
                                    //}
                                    //else
                                    //{
                                    //    //Console.WriteLine(
                                    //    //    "MISS -> "+Path.GetFileName(imagePath)
                                    //    //    + ": Word Path : "
                                    //    //    + wordPath
                                    //    //    + " , Word Text : "
                                    //    //    + Ocr.Read(wordPath).Text
                                    //    //);
                                    //}
                                    //if (nextImage) break;
                                }
                                //if (nextImage) break;
                            }
                            //if (nextImage) break;
                        }
                    //if (nextImage) break;
                        Match m = Regex.Match(Ocr.Read(imagePath).Text, regEx);
                        if (m.Success)
                            Console.WriteLine("File: {1} Match : {0}", Regex.Match(page.Text, regEx), Path.GetFileName(imagePath));    
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
