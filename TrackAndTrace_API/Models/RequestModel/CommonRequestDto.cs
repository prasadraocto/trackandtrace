namespace TrackAndTrace_API.Models.RequestModel
{
    public class CommonRequestDto
    {
        public int page { get; set; } = 1;
        public int page_size { get; set; } = 10;
        public string? search_query { get; set; } = "";
        public string? sort_column { get; set; } = "id";
        public string? sort_direction { get; set; } = "desc";
        public bool? is_export { get; set; } = false;
    }
}
