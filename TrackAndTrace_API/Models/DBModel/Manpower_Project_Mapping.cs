namespace TrackAndTrace_API.Models.DBModel
{
    public class Manpower_Project_Mapping
    {
        public int id { get; set; }
        public int manpower_id { get; set; }
        public int project_id { get; set; }
        public bool delete_flag { get; set; } = false;
    }
}