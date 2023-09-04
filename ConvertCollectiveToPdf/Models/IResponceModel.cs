namespace ConvertCollectiveToPdf.Models
{
    public interface IResponceModel
    {

    }
   
    public class SuccessResponse : IResponceModel
    {
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
    }
    public class SuccessResponseWithContent : SuccessResponse
    {
        public Object Content { get; set; }
    }
    public class SuccessResponseWithOutPutFile : SuccessResponse
    {
        public string OutPutFile { get; set; }  
        public string TimeElapsed { get; set; }
    }
}
