namespace TrackAndTrace_API.Models.DBModel
{
    public class Machinery : CommonDBDto
    {
        public decimal quantity { get; set; }
        public bool in_house { get; set; }
    }
}