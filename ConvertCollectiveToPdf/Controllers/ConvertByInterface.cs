using ConvertCollectiveToPdf.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq;
using System.Text;

namespace ConvertCollectiveToPdf.Controllers
{

    [ApiController]
    [Route("convert_interface")]
    public class ConvertByInterface : Controller
    {
        private readonly IConfiguration _configuration;

        public ConvertByInterface(IConfiguration configuration )
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("get_all_collective")]
        public async Task<ActionResult<IResponceModel>> GetAllCollective(string htmlFilePath)
        {
            string apiUrl = "https://localhost:7052//convert_document//collective_to_pdf";
            HttpClient ConvertCollectiveToPdf = new HttpClient();
            var postData = new FileModel()
            {
                InputHtmlFile = htmlFilePath,
                Descripsion = "",
                OutputPdfFile = ""
            };
            string dataToPost = JsonConvert.SerializeObject(postData);

            var url = new StringContent(dataToPost, Encoding.UTF8, "application/json");
            HttpContent content = url;

            var reponce = await ConvertCollectiveToPdf.PostAsync(apiUrl, content);
            Console.WriteLine("Status code is :- " + reponce.StatusCode.ToString());

            return Ok(new SuccessResponse()
            {
                ResponseCode = reponce.StatusCode.ToString(),
                ResponseMessage = "Api url is not correct "
            });

            return Ok( new SuccessResponseWithContent()
            {
                Content = reponce.ToString(),
                ResponseCode = "0" ,
                ResponseMessage = "Hello"
            });
        }

       /* [HttpPost]
        [Route("convert_to_pdf")]
        public ActionResult<IResponceModel> ConvertSpecificCollective(string collectiveName)
        {
            try
            {
                StringBuilder collectiveFilePath = new StringBuilder(ListOfCollectivePath).Append("\\").Append(collectiveName);

                // Use It Later 
                var collective = _listOfCollectives.Where(s => s.Equals(collectiveName)).FirstOrDefault();  

                if (!System.IO.File.Exists(collectiveFilePath.ToString()) )
                {
                    throw new FileNotFoundException($"File {collectiveName} Not Found in the Directory ");
                }
                HttpClient httpClient = new HttpClient();   
                
                // HttpClient must be Here 
                return Ok();
            }

            catch (Exception ex)
            {
               return NotFound(
                 new SuccessResponse() { 
                    ResponseCode = "1" , 
                    ResponseMessage = ex.Message   
                });
            }
        }*/


       /* [HttpPost]
        [Route("filter_result")]
        public ActionResult FilterResult(string CollectiveName)
        {

            var item = (from serchString in _listOfCollectives
                        where serchString.StartsWith(CollectiveName)
                        select serchString).ToList();

            if (item != null || item.Count() > 0)
            {
                return Ok(new SuccessResponseWithContent()
                {
                    ResponseCode = "0",
                    ResponseMessage = "Success responce",
                    Content = item
                });
            }

            else
            {
                return Ok(new SuccessResponse()
                {
                    ResponseCode = "1",
                    ResponseMessage = "There is no file like this name ",
                });
            }
        }*/
    }
}
