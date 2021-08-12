using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using IronOcr;


namespace OCRtest
{
    class Program
    {
        //private static readonly string cutImagesPath= "C:/Users/Public/Picturesimages/cut_images/";
        private static readonly string finalImagePath= "C:/Users/Public/Picturesimages/final_images/";
        private static readonly String regEx = "[0-9]+-[0-9]+-[0-9]+";
        
        //se debe cambiar este directorio
        private static readonly string sourceFiles = "C:/Users/evidela/OneDrive - ANDREANI LOGISTICA SA/Escritorio/capturasVideo";

        static void Main(string[] args)
        {
            var Ocr = new IronTesseract();
            //Ocr.Configuration.ReadBarCodes = false;
            var i = 0;
            string[] imagePathArray = Directory.GetFiles(sourceFiles);
            ManageDirectories();

            foreach (var imagePath in imagePathArray)
            {
                using (var Input = new OcrInput(imagePath))
                {
                    //Input.Binarize();
                    OcrResult result = Ocr.Read(Input);

                    foreach (var page in result.Pages)
                    {
                        MatchCollection mc = Regex.Matches(Ocr.Read(imagePath).Text, regEx);
                        
                        foreach (Match m in mc) {
                            
                            if (m.Success)
                            {
                                Console.WriteLine("File: {1} Match : {0}", m.Value, Path.GetFileName(imagePath));
                                var name = m.Value;
                                Image img = Image.FromFile(imagePath);
                                img.Save(finalImagePath + m.Value +"_"+ Guid.NewGuid().ToString().Substring(0,3)+ ".jpg");
                                i++;
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
        }
    }
}
