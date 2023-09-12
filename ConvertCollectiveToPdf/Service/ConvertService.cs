using ConvertCollectiveToPdf.Controllers;
using ConvertCollectiveToPdf.Models;
using DinkToPdf;
using DinkToPdf.Contracts;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using PdfSharp;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.IO;

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

        public ConvertService()
        {

        }
        public string[] GetHtmlPages(string htmlFilePath)
        {

            try
            {
                _logger.LogInformation("... Start Getting Html pages...");

                string htmlContent = System.IO.File.ReadAllText(htmlFilePath);
                string[] htmlPages = htmlContent.Split(new[] { _configuration["ConvertToPdfVariable:SplitFlag"] }, StringSplitOptions.RemoveEmptyEntries);
                _logger.LogInformation("... Finished Getting Html pages...");

                return htmlPages;
            }

            catch (Exception ex)
            {
                throw new NullReferenceException("The split flag not found in the configuration file ");
            }
        }

        public void MergePDF(string InputDirectoryPath, string OutputPdfPath)
        {
            try
            {
                _logger.LogInformation($"... Start MergePDF To One PDF File  ...");

                iTextSharp.text.pdf.PdfReader reader = null;
                Document sourceDocument = null;
                PdfCopy pdfCopyProvider = null;
                PdfImportedPage importedPage;


                sourceDocument = new Document();
                pdfCopyProvider = new PdfCopy(sourceDocument, new System.IO.FileStream(OutputPdfPath, System.IO.FileMode.Create));

                // Open the output file   
                sourceDocument.Open();

                var listOfPDFInvoices = Directory.GetFiles(InputDirectoryPath);

                var listOfPDFSorted = listOfPDFInvoices.OrderBy(collectiveFile =>
                {
                    string fileNameWithout = Path.GetFileNameWithoutExtension(collectiveFile);
                    var collectiveIndex = new String(fileNameWithout.SkipWhile(c => !Char.IsDigit(c)).TakeWhile(c => Char.IsDigit(c)).ToArray());
                    return int.Parse(collectiveIndex);
                }).ToArray();

                if (!Directory.Exists(InputDirectoryPath))
                {
                    throw new DirectoryNotFoundException("Directory not found ");
                }

                //Loop through the files list
                for (int pdfDocument = 0; pdfDocument < listOfPDFSorted.Length; pdfDocument++)
                {
                    reader = new iTextSharp.text.pdf.PdfReader(listOfPDFSorted[pdfDocument]);
                    importedPage = pdfCopyProvider.GetImportedPage(reader, 1);
                    pdfCopyProvider.AddPage(importedPage);
                    reader.Close();
                }
                sourceDocument.Close();
                _logger.LogInformation("... Finished MergePDF To One pDF File ...");
            }

            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public Steps CheckSteps(string inputHtmlFile, string outputFileDirectory, string outputFilePath, string[] listOfPagesToCompare)
        {
            Steps step = Steps.StartConvert;
            _logger.LogInformation("... Start CheckSteps ...");

            if (Directory.GetFiles(outputFileDirectory).Count() == listOfPagesToCompare.Count() - 1)
            {
                step = Steps.StartCombine;
            }

            else if (System.IO.File.Exists(outputFilePath) && Directory.Exists(outputFileDirectory))
            {
                step = Steps.DeleteDirectory;
            }

            _logger.LogInformation("... finished CheckSteps ...");

            return step;
        }

        public async Task DeleteFileFromDirectory(string filePath)
        {
            if (!Directory.Exists(filePath))
            {
                throw new Exception($" {filePath} doesnt exist ");
            }
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
                _logger.LogInformation($"... Start Convert {convertor.PageName} Html To Pdf , thread id :- {Thread.CurrentThread.ManagedThreadId}");

                var doc = new HtmlToPdfDocument()
                {
                    GlobalSettings =

                                {
                                    ColorMode = ColorMode.Color,
                                    Orientation = convertor.IsChild ? Orientation.Landscape : Orientation.Portrait,
                                    PaperSize = PaperKind.A4,
                                    ImageDPI=350,
                                    ImageQuality=60,
                                    Out = convertor.OutputPdfFile+"\\"+convertor.PageName+".pdf",
                                    Margins = convertor.IsChild ? new MarginSettings() {
                                        Top = 2,
                                        Bottom = 0,
                                        Right = 3.4 , 
                                        Left = 3.4
                                    } 
                                    :new MarginSettings() {
                                        Top = 3.5,
                                        Bottom = 0,
                                        Right = 1.4 ,
                                        Left = 1.4
                                    },                                    
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
                _logger.LogInformation($"... Finished Convert {convertor.PageName} Html To Pdf , thread id :- {Thread.CurrentThread.ManagedThreadId}");
            }

            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
