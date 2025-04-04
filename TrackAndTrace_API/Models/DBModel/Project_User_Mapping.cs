namespace TrackAndTrace_API.Models.DBModel
{
    public class Project_User_Mapping
    {
        public int id { get; set; }
        public int project_id { get; set; }
        public int user_id { get; set; }
        public bool delete_flag { get; set; } = false;
    }
}