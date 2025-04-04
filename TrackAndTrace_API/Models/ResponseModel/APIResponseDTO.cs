namespace TrackAndTrace_API.Models.ResponseModel
{
    public class APIResponseDTO
    {
        public bool success { get; set; } = false;
        public string? message { get; set; } = "Unauthorized";
        public Object? data { get; set; } = null;
        public int? total { get; set; }
        public int? page { get; set; }
        public int? page_size { get; set; }
    }
    public class BulkImportDetailsResponseDTO
    {
        public bool success { get; set; } = false;
        public string? message { get; set; } = "Unauthorized";
        public Object? data { get; set; } = null;
        public int? total { get; set; }
        public int? page { get; set; }
        public int? page_size { get; set; }
    }
}
