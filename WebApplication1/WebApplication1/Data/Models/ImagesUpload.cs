namespace WebApplication1.Data.Models
{
    public class ImagesUploadForOrders
    {
        public int OrderID { get; set; }
        public string Filename { get; set; }
        public byte[] Data { get; set; }
    }
    public class ImagesUploadForReports
    {
        public int ReportID { get; set; }
        public string Filename { get; set; }
        public byte[] Data { get; set; }
    }
    public class ImagesUploadForProducts
    {
        public int ProductID { get; set; }
        public string Filename { get; set; }
        public byte[] Data { get; set; }
    }
}
