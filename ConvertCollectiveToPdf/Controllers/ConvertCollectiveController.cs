using ConvertCollectiveToPdf.Models;
using ConvertCollectiveToPdf.Service;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.IO;

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
                var step = Steps.StartConvert;
                var listOfPagesToCompare = _convertService.GetHtmlPages(fileInput.InputHtmlFile);

                _logger.LogInformation("... Start Convert Collective Html To Pdf ...");

                var index = 0;
                var outputFileDirectory = _configuration["ConvertToPdfVariable:OutputPath"];
                var collectiveName = Path.GetFileNameWithoutExtension(fileInput.InputHtmlFile);
                outputFileDirectory = outputFileDirectory + "\\" + collectiveName;

                if (!Directory.Exists(outputFileDirectory))
                {
                    Directory.CreateDirectory(outputFileDirectory);
                }

                if (!System.IO.File.Exists(fileInput.InputHtmlFile))
                {
                    throw new FileNotFoundException("Collective Html file not exist ");
                }

                if (Directory.GetFiles(outputFileDirectory).Count() == listOfPagesToCompare.Count() )
                {
                    step = Steps.StartCombine;
                }


               if (step == Steps.StartConvert)
                {
                    var listOfPages = _convertService.GetHtmlPages(fileInput.InputHtmlFile);
                    stylePage = listOfPages[0];

                    foreach (var page in listOfPages)
                    {
                        StringBuilder childDocument = new StringBuilder().Append(stylePage);
                        //First Page (Base) ----> portrait
                        if (index == 0)
                        {
                            childDocument.Append(page);
                            childDocument.Append(endDocument);

                            _convertService.ConvertEachPageToPdf(new PdfConvertor()
                            {
                                OutputPdfFile = outputFileDirectory,
                                PageContent = childDocument.ToString(),
                                PageName = new string("base_" + collectiveName),
                                IsChild = false
                            });
                        }
                        // Child Page ----> landscape
                        else if (index != 0 && index != listOfPages.Length)
                        {
                            childDocument.Append(page);
                            childDocument.Append(endDocument);

                            _convertService.ConvertEachPageToPdf(new PdfConvertor()
                            {
                                OutputPdfFile = outputFileDirectory,
                                PageContent = childDocument.ToString(),
                                PageName = new string("childDetail_" + index),
                                IsChild = true
                            });
                        }

                        // Total page ---> landscape
                        if (index == listOfPages.Length)
                        {
                            childDocument.Append(page);
                            _convertService.ConvertEachPageToPdf(new PdfConvertor()
                            {
                                OutputPdfFile = outputFileDirectory,
                                PageContent = childDocument.ToString(),
                                PageName = new string("childDetail_" + index),
                                IsChild = true
                            });
                        }
                        index++;
                    }

                    index = 0;
                    step = Steps.StartCombine;
                }

                if (step == Steps.StartCombine)
                {
                    StringBuilder outputFilePath = new StringBuilder(_configuration["ConvertToPdfVariable:OutputPath"]);
                    outputFilePath.Append("\\").Append(Path.GetFileNameWithoutExtension(fileInput.InputHtmlFile)).Append(".pdf");

                    if (Directory.GetFiles(outputFileDirectory).Count() == 0 )
                    {
                        return Ok(
                       new SuccessResponce()
                       {
                           ResponceCode = "1",
                           ResponceMessage = $"the file {collectiveName} not contain any pdf file to compine it "
                       });
                    }
                        // Compine Pdf files to the single file 
                        _convertService.MergePDFs(outputFileDirectory, outputFilePath.ToString());

                    return Ok(
                        new SuccessResponce()
                        {
                            ResponceCode = "0",
                            ResponceMessage = $"done convert {collectiveName} to pdf"
                        });
                }
                else
                {
                    return Ok(
                    new SuccessResponce()
                    {
                        ResponceCode = "0",
                        ResponceMessage = $"done convert {collectiveName} to pdf"
                    });
                
                }
            }
            catch (Exception ex)
            {

                return NotFound(
                    new SuccessResponce()
                    {
                        ResponceCode = "1",
                        ResponceMessage = ex.Message,
                    });
            }

           }
        }
    }

