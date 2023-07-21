namespace ConvertCollectiveToPdf.Models
{
    public interface IResponceModel
    {

    }
   
    public class SuccessResponce : IResponceModel
    {
        public string ResponceCode { get; set; }
        public string ResponceMessage { get; set; }
    }
    public class SuccessResponceWithContent : SuccessResponce
    {
        public Object Content { get; set; }
    }
}
