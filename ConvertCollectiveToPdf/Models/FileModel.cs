using System.Runtime.InteropServices;

namespace ConvertCollectiveToPdf.Models
{
    public class FileModel
    {
        public string InputHtmlFile { get; set; }    
        public string? OutputPdfFile { get; set; } 
        public string? MwNotification { get; set; }
        public string? Descripsion { get; set; }   
    }
}
