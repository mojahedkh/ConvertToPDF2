using ConvertCollectiveToPdf.Models;
using ConvertCollectiveToPdf.Service;
using DinkToPdf;
using DinkToPdf.Contracts;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.IO;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Concurrent;
using System;

namespace ConvertCollectiveToPdf.Controllers
{

    [ApiController]
    [Route("convert_document")]
    public class ConvertCollectiveController : Controller
    {
        private IConverter _converter;
        private readonly ILogger<ConvertCollectiveController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ConvertService _convertService;
        public static string endDocument;
        public static string stylePage; 

        public ConvertCollectiveController(IConverter converter, IConfiguration configuration, ILogger<ConvertCollectiveController> logger , ConvertService convertService)
        {
            _logger = logger;
            _converter = converter;
            _configuration = configuration;
            _convertService = convertService;
            endDocument = _configuration["ConvertToPdfVariable:EndDocument"]; 
        }

        [HttpPost]
        [Route("collective_to_pdf")]
        public ActionResult<IResponceModel> ConvertCollective(FileModel fileInput)
        {
            try
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                
                var index = 0;
                var defaultOutFile = _configuration["ConvertToPdfVariable:OutputPath"];
                var collectiveName = Path.GetFileNameWithoutExtension(fileInput.InputHtmlFile);
                string outputFileDirectory = ((fileInput.OutputPdfFile.Length==0 || fileInput.OutputPdfFile == null ) ? defaultOutFile : fileInput.OutputPdfFile) + "\\" + collectiveName;
                // Check if Directory Not Exists 
                if (!Directory.Exists(outputFileDirectory))
                {
                    Directory.CreateDirectory(outputFileDirectory); 
                }

                StringBuilder outputFilePath = new StringBuilder(outputFileDirectory).Append(collectiveName).Append(".pdf");

                if (!System.IO.File.Exists(fileInput.InputHtmlFile))
                {
                    throw new FileNotFoundException("Collective Html file not exist ");
                }

                var listOfPagesToCompare = _convertService.GetHtmlPages(fileInput.InputHtmlFile);
                var step  = _convertService.CheckSteps(fileInput.InputHtmlFile , outputFileDirectory , outputFilePath.ToString() , listOfPagesToCompare);

                if (step == Steps.StartConvert)
                {
                    stylePage = listOfPagesToCompare[0];
                    _logger.LogInformation("... Start Convert Collective Html To Pdf ...");

                    string pageName;
                    StringBuilder childDocument = new StringBuilder();

                    foreach (var page in listOfPagesToCompare)
                    {
                        childDocument.Clear();
                        childDocument.Length = 0;
                        childDocument.Append(stylePage);

                        if (index > 1 && index != listOfPagesToCompare.Length-1 )
                        {
                            pageName = new string("childPage_" + index);

                            childDocument.Append(page);
                            childDocument.Append(endDocument);

                            PdfConvertor pdfConverter = new PdfConvertor()
                            {
                                OutputPdfFile = outputFileDirectory,
                                PageContent = childDocument.ToString(),
                                PageName = pageName,
                                IsChild = true
                            };

                             _convertService.ConvertEachPageToPdf(pdfConverter);

                        }

                        else if (index == 1)
                        {
                            pageName = new string("base_" + index);
                            //First Page (Base) ----> portrait
                            childDocument.Append(page);
                            childDocument.Append(endDocument);

                            _convertService.ConvertEachPageToPdf(new PdfConvertor()
                            {
                                OutputPdfFile = outputFileDirectory,
                                PageContent = childDocument.ToString(),
                                PageName = pageName,
                                IsChild = false
                            });
                        }
                        index++;
                    }

                    _logger.LogInformation("... Finished Convert Collective Html To Pdf ...");

                    index = 0;
                    step = Steps.StartCombine;
                }

                if (step == Steps.StartCombine)
                {

                    if (Directory.GetFiles(outputFileDirectory).Count() == 0)
                    {
                        return Ok(
                       new SuccessResponse()
                       {
                           ResponseCode = "1",
                           ResponseMessage = $"the file {collectiveName} not contain any pdf file to compine it "
                       });
                    }
                     // Compine Pdf files to the single file 
                     _convertService.MergePDF(outputFileDirectory, outputFilePath.ToString());
                     step = Steps.DeleteDirectory;
                }

                if (step == Steps.DeleteDirectory)
                {
                    _convertService.DeleteFileFromDirectory(outputFileDirectory);
                    watch.Stop();

                    return Ok(
                    new SuccessResponseWithOutPutFile()
                    {
                        ResponseCode = "0",
                        ResponseMessage = $"Done convert {collectiveName} to pdf",
                        OutPutFile = outputFilePath.ToString(),
                        TimeElapsed = $"{(watch.ElapsedMilliseconds * 0.001).ToString()}/ second",
                    });
                }
                else
                {
                    return Ok(
                    new SuccessResponse()
                    {
                        ResponseCode = "0",
                        ResponseMessage = $"Done convert {collectiveName} to pdf"
                    });
                }
            }
            catch (Exception ex)
            {

                return NotFound(
                    new SuccessResponse()
                    {
                        ResponseCode = "1",
                        ResponseMessage = ex.Message,
                    });
            }
           }

        [HttpGet]
        [Route("test")]
        public string test()
        {
            return "test";
        }
        }
    }

