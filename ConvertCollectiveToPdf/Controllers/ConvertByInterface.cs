using ConvertCollectiveToPdf.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Text;

namespace ConvertCollectiveToPdf.Controllers
{

    [ApiController]
    [Route("convert_interface")]
    public class ConvertByInterface : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly String ListOfCollectivePath;
        private readonly String[] _listOfCollectives;

        public ConvertByInterface(IConfiguration configuration , String[] listOfCollectives)
        {
            _configuration = configuration;
            _listOfCollectives = listOfCollectives;
            ListOfCollectivePath = _configuration["ConvertToPdfVariable:CollectiveFolder"];
        }

        [HttpGet]
        [Route("get_all_collective")]
        public ActionResult<IResponceModel> GetAllCollective()
        {
            if (_listOfCollectives != null && _listOfCollectives.Count()!= 0 ) {
                return Ok(new SuccessResponceWithContent()
                {
                    ResponceCode = "0" , 
                    ResponceMessage ="Data returned Succesfully" , 
                    Content = this._listOfCollectives
                });
            }
            else
            {
                return NotFound(new SuccessResponce()
                {
                    ResponceCode = "1" ,
                    ResponceMessage = "No data Found "
                });
            }
        }

        [HttpPost]
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
                 new SuccessResponce() { 
                    ResponceCode = "1" , 
                    ResponceMessage = ex.Message   
                });
            }
        }

        [HttpPost]
        [Route("filter_result")]
        public ActionResult FilterResult(string CollectiveName)
        {
            var item = (from serchString in _listOfCollectives
                        where serchString.StartsWith(CollectiveName)
                        select serchString).ToList();

            if (item != null || item.Count() > 0)
            {
                return Ok(new SuccessResponceWithContent()
                {
                    ResponceCode = "0",
                    ResponceMessage = "Success responce",
                    Content = item
                });
            }

            else
            {
                return Ok(new SuccessResponce()
                {
                    ResponceCode = "1",
                    ResponceMessage = "There is no file like this name ",
                });
            }
        }
    }
}
