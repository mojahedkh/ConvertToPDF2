namespace ConvertCollectiveToPdf.Models
{
    public class PdfConvertor
    {
        public string OutputPdfFile { get; set; }
        public string PageName { get; set; }
        public string PageContent { get; set; }
        
        public Boolean IsChild { get; set; }
        
    }
}
