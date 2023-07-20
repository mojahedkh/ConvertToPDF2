using ConvertCollectiveToPdf.Controllers;
using ConvertCollectiveToPdf.Models;
using DinkToPdf;
using DinkToPdf.Contracts;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;

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

            string htmlContent = System.IO.File.ReadAllText(htmlFilePath);
            string[] htmlPages = htmlContent.Split(new[] { _configuration["ConvertToPdfVariable:SplitFlag"] }, StringSplitOptions.RemoveEmptyEntries);
            return htmlPages;
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

        public void CombinePDFs(string outputFilePath, string inputFile)
        {

            using (FileStream outputStream = new FileStream(outputFilePath, FileMode.Create))
            {
                PdfDocument document = new PdfDocument();
                PdfCopy pdfCopy = new PdfCopy(document, outputStream);
                var inputFilePaths = Directory.GetFiles(inputFile);

                try
                {
                    document.Open();

                    foreach (string inputFilePath in inputFilePaths)
                    {

                        PdfReader pdfReader = new PdfReader(inputFilePath);
                        PdfImportedPage importedPage = pdfCopy.GetImportedPage(pdfReader, 1);
                        pdfCopy.AddPage(importedPage);
                        pdfReader.Close();

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error combining PDFs: " + ex.Message);
                }
                finally
                {
                }
            }

        }

        public void ConvertEachPageToPdf(PdfConvertor convertor)
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
    }
}
