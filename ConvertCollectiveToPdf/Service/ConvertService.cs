using ConvertCollectiveToPdf.Controllers;
using ConvertCollectiveToPdf.Models;
using DinkToPdf;
using DinkToPdf.Contracts;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using PdfSharp;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace ConvertCollectiveToPdf.Service
{
    public class ConvertService
    {
        private IConverter _converter;
        private readonly ILogger<ConvertService> _logger;
        private readonly IConfiguration _configuration;
        public ConvertService(IConverter converter, IConfiguration configuration, ILogger<ConvertService> logger)
        {
            _logger = logger;
            _converter = converter;
            _configuration = configuration;
        }

        public string[] GetHtmlPages(string htmlFilePath)
        {

         try
          {
            string htmlContent = System.IO.File.ReadAllText(htmlFilePath);
            string[] htmlPages = htmlContent.Split(new[] { _configuration["ConvertToPdfVariable:SplitFlag"] }, StringSplitOptions.RemoveEmptyEntries);
            return htmlPages;
          }

            catch (Exception ex) {
                throw new NullReferenceException("The split flag not found in the configuration file ");
            }
        }

        private void CombineMultiplePdfFileToSingleOne(string InputDirectoryPath, string OutputFilePath)
        {
            string[] inputFilePaths = Directory.GetFiles(InputDirectoryPath);

            using (var outputStream = System.IO.File.Create(OutputFilePath))
            {
                foreach (var inputFilePath in inputFilePaths)
                {
                    using (var inputStream = System.IO.File.OpenRead(inputFilePath))
                    {
                        inputStream.CopyTo(outputStream);
                    }
                }
            }
        }

        public void MergePDFs(string InputDirectoryPath,  string OutputFilePath)
        {
            try
            {
                if (!Directory.Exists(InputDirectoryPath))
                {
                    throw new DirectoryNotFoundException("Directory not found ");
                }

           /*     var listOfPDFinvoices = Directory.GetFiles(InputDirectoryPath);

                using (var targetDoc = new PdfSharp.Pdf.PdfDocument())
                {

                    foreach (var pdf in listOfPDFinvoices)
                    {
                        using (var pdfDoc = PdfSharp.Pdf.IO.PdfReader.Open(pdf, PdfDocumentOpenMode.Import))
                        {
                            targetDoc.AddPage(pdfDoc.Pages[0]);
                        }
                    }
                    targetDoc.Save(OutputFilePath);
                }*/

                DeleteFileFromDirectory(InputDirectoryPath); 
            }
            catch(Exception ex) { 

            }
        }
        
        public void DeleteFileFromDirectory(string filePath)
        {
            var ListPfPdf = Directory.GetFiles(filePath);  
            
            foreach (var file in ListPfPdf)
            {
                File.Delete(file);
            }
            Directory.Delete(filePath);
        }

        public void ConvertEachPageToPdf(PdfConvertor convertor)
        {
            try
            {
                var doc = new HtmlToPdfDocument()
                {
                    GlobalSettings =
                                {
                                    ColorMode = ColorMode.Color,
                                    Orientation = convertor.IsChild ? Orientation.Landscape : Orientation.Portrait,
                                    PaperSize = PaperKind.A4,
                                    Margins = new MarginSettings()
                                    {
                                        Top = 12, Bottom = 6, Left = 5, Right = 5
                                    },
                                    Out = convertor.OutputPdfFile+"\\"+convertor.PageName+".pdf"
                                },

                        Objects =
                                {
                                    new ObjectSettings()
                                        {
                                            HtmlContent = convertor.PageContent,//htmlDoc.DocumentNode.OuterHtml,
                                            WebSettings = {  DefaultEncoding = "utf-8",},
                                        }
                                }
                };

                _converter.Convert(doc);
                _logger.LogInformation("convert done.");
            }

            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }
    }
}
