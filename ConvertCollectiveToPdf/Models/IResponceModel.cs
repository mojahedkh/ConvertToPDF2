namespace ConvertCollectiveToPdf.Models
{
    public interface IResponceModel
    {

    }
    public class ErrorResponce : IResponceModel
    {
        public String ErrorMessage { get; set; }
        public String ErrorCode { get; set; }
    }
    public class SuccessResponce : IResponceModel
    {
        public string ResponceCode { get; set; }
        public string ResponceMessage { get; set; }
    }
}
