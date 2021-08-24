using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using IronOcr;

namespace OCRtest
{
    class Program
    {
        private static readonly string cutImagesPath= "C:/Users/Public/Picturesimages_3/cut_images/";
        private static readonly string finalImagePath= "C:/Users/Public/Picturesimages_3/final_images/";
        private static readonly string failImagePath = "C:/Users/Public/Picturesimages_3/fail_images/";
        private static readonly String regEx = "[0-9]+-[0-9]+-[0-9]+";
        private static readonly string regEx2 = "[0-9][0-9][0-9] [0-9][0-9][0-9] [0-9][0-9][0-9]";

        //se debe cambiar este directorio
        private static readonly string sourceFiles = "C:/Users/evidela/OneDrive - ANDREANI LOGISTICA SA/Escritorio/images2";

        static void Main(string[] args)
        {
            var Ocr = new IronTesseract();
            MatchCollection mc;
            Ocr.Configuration.ReadBarCodes = false;
            bool hit;
            
            string[] imagePathArray = Directory.GetFiles(sourceFiles);

            ManageDirectories();

            foreach (var imagePath in imagePathArray)
            {
                hit = false;
                Console.WriteLine("Processing "+Path.GetFileName(imagePath)+" ...");
                using (var Input = new OcrInput(imagePath))
                {
                    OcrResult result = Ocr.Read(Input);
                    mc = Regex.Matches(result.Text, regEx);
                    foreach (Match m in mc)
                    {
                        if (m.Success)
                        {
                            hit = saveImageWithLocation(imagePath, m);
                        }
                    }
                    if (!hit)
                    {
                        foreach (var page in result.Pages)
                            {
                                Console.WriteLine("Trying Page {0} ...", page.PageNumber);
                                string wordPath =
                                                cutImagesPath
                                                + Path.GetFileNameWithoutExtension(imagePath)
                                                + "_cut_"
                                                + page.PageNumber
                                                + ".png";
                                page.ToBitmap(Input).Save(wordPath);
                                //Match m = Regex.Match(Ocr.Read(wordPath).Text, regEx);
                                //aca estoy leyendo a nivel de pagina , tengo que leer a nivel de imagen antes y despues voy bajando
                                mc = Regex.Matches(Ocr.Read(wordPath).Text, regEx);
                                foreach (Match m in mc)
                                {
                                    if (m.Success)
                                    {
                                        hit = saveImageWithLocation(imagePath, m);
                                    }
                                    if (hit) break;
                                }
                                if (!hit)
                                {

                                    foreach (var paragraph in page.Paragraphs)
                                    {
                                        Console.WriteLine("Trying Page {0} Paragraph {1}", page.PageNumber, paragraph.ParagraphNumber);
                                        wordPath =
                                               cutImagesPath
                                               + Path.GetFileNameWithoutExtension(imagePath)
                                               + "_cut_"
                                               + page.PageNumber
                                               + paragraph.ParagraphNumber
                                               + ".png";
                                        paragraph.ToBitmap(Input).Save(wordPath);

                                        mc = Regex.Matches(Ocr.Read(wordPath).Text, regEx);
                                        foreach (Match m in mc)
                                        {
                                            if (m.Success)
                                            {
                                                hit = saveImageWithLocation(imagePath, m);
                                            }
                                            if (hit) break;
                                        }
                                        if (!hit)
                                        {
                                            foreach (var line in paragraph.Lines)
                                            {
                                                Console.WriteLine("Trying Page {0} Paragraph {1} Line {2}", page.PageNumber, paragraph.ParagraphNumber, line.LineNumber);
                                                wordPath =
                                                cutImagesPath
                                                + Path.GetFileNameWithoutExtension(imagePath)
                                                + "_cut_"
                                                + page.PageNumber
                                                + paragraph.ParagraphNumber
                                                + line.LineNumber
                                                + ".png";
                                                line.ToBitmap(Input).Save(wordPath);

                                                mc = Regex.Matches(Ocr.Read(wordPath).Text, regEx);
                                                foreach (Match m in mc)
                                                {
                                                    if (m.Success)
                                                    {
                                                        hit = saveImageWithLocation(imagePath, m);
                                                    }
                                                    if (hit) break;
                                                }
                                                if (!hit)
                                                {
                                                    foreach (var word in line.Words)
                                                    {
                                                        Console.WriteLine("Trying Page {0} Paragraph {1} Line {2} Word {3}", page.PageNumber, paragraph.ParagraphNumber, line.LineNumber, word.WordNumber);
                                                        wordPath =
                                                            cutImagesPath
                                                            + Path.GetFileNameWithoutExtension(imagePath)
                                                            + "_cut_"
                                                            + page.PageNumber
                                                            + paragraph.ParagraphNumber
                                                            + line.LineNumber
                                                            + word.WordNumber
                                                            + ".png";
                                                        word.ToBitmap(Input).Save(wordPath);
                                                        mc = Regex.Matches(Ocr.Read(wordPath).Text, regEx);
                                                        foreach (Match m in mc)
                                                        {
                                                            if (m.Success)
                                                            {
                                                                hit = saveImageWithLocation(imagePath, m);
                                                            }
                                                            if (hit) break;
                                                        }
                                                    }
                                                    if (hit) break;
                                                }
                                            }
                                            if (hit) break;
                                        }
                                    }
                                    if (hit) break;
                                }
                                if (hit) break;
                            }
                        
                        if (hit) break;
                    }
                    
                }
                if (!hit)
                {
                    saveImageWithNoLocation(imagePath);
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
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("File: {1} Match : {0}", m.Value, Path.GetFileName(imagePath));
            Console.ResetColor();
            var name = m.Value;
            Image img = Image.FromFile(imagePath);
            img.Save(finalImagePath + m.Value + "_" + Guid.NewGuid().ToString().Substring(0, 4) + ".png");
            return true;
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
        }
    }
}
