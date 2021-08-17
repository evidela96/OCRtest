using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using IronOcr;

namespace OCRtest
{
    class Program
    {
        private static readonly string cutImagesPath= "C:/Users/Public/Picturesimages_2/cut_images/";
        private static readonly string finalImagePath= "C:/Users/Public/Picturesimages_2/final_images/";
        private static readonly string failImagePath = "C:/Users/Public/Picturesimages_2/fail_images/";
        private static readonly String regEx = "[0-9]+-[0-9]+-[0-9]+";

        //se debe cambiar este directorio
        private static readonly string sourceFiles = "C:/Users/evidela/OneDrive - ANDREANI LOGISTICA SA/Escritorio/images";

        static void Main(string[] args)
        {
            var Ocr = new IronTesseract();
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
                        MatchCollection mc = Regex.Matches(Ocr.Read(wordPath).Text, regEx);
                        foreach (Match m in mc)
                        {
                            if (m.Success)
                            {
                                hit = true;
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("PAGE LEVEL : File: {1} Match : {0}", m.Value, Path.GetFileName(imagePath));
                                Console.ResetColor();
                                var name = m.Value;
                                Image img = Image.FromFile(imagePath);
                                img.Save(finalImagePath + m.Value + "_" + Guid.NewGuid().ToString().Substring(0, 4) + ".png");
                            }
                            if (hit) break;
                        }
                        if (!hit) {
                            
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
                                foreach( Match m in mc)
                                {
                                    if (m.Success)
                                    {
                                        hit = true;
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine("PARAGRAPH LEVEL : File: {1} Match : {0}", m.Value, Path.GetFileName(imagePath));
                                        Console.ResetColor();
                                        var name = m.Value;
                                        Image img = Image.FromFile(imagePath);
                                        img.Save(finalImagePath + m.Value + "_" + Guid.NewGuid().ToString().Substring(0, 4) + ".png");
                                    }
                                    if (hit) break;
                                }
                                if (!hit)
                                {
                                    foreach (var line in paragraph.Lines) {
                                        Console.WriteLine("Trying Page {0} Paragraph {1} Line {2}", page.PageNumber, paragraph.ParagraphNumber,line.LineNumber);
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
                                                hit = true;
                                                Console.ForegroundColor = ConsoleColor.Green;
                                                Console.WriteLine("LINE LEVEL : File: {1} Match : {0}", m.Value, Path.GetFileName(imagePath));
                                                Console.ResetColor();
                                                var name = m.Value;
                                                Image img = Image.FromFile(imagePath);
                                                img.Save(finalImagePath + m.Value + "_" + Guid.NewGuid().ToString().Substring(0, 4) + ".png");
                                            }
                                            if (hit) break;
                                        }
                                        if (!hit)
                                        {    
                                            foreach (var word in line.Words) {
                                                Console.WriteLine("Trying Page {0} Paragraph {1} Line {2} Word {3}", page.PageNumber, paragraph.ParagraphNumber, line.LineNumber,word.WordNumber);
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
                                                        hit = true;
                                                        Console.ForegroundColor = ConsoleColor.Green;
                                                        Console.WriteLine("WORD LEVEL : File: {1} Match : {0}", m.Value, Path.GetFileName(imagePath));
                                                        Console.ResetColor();
                                                        var name = m.Value;
                                                        Image img = Image.FromFile(imagePath);
                                                        img.Save(finalImagePath + m.Value + "_" + Guid.NewGuid().ToString().Substring(0, 4) + ".png");
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
                }
                if (!hit)
                {
                    Image m = Image.FromFile(imagePath);
                    m.Save(failImagePath + Path.GetFileNameWithoutExtension(imagePath) + "_"+ Guid.NewGuid().ToString().Substring(0, 4) + ".png");
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
            if (!Directory.Exists(failImagePath))
            {
                Directory.CreateDirectory(failImagePath);
            }
        }
    }
}
