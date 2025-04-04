using TrackAndTrace_API.Models.DBModel;

namespace TrackAndTrace_API.Models.RequestModel
{
    public class ProjectLevelDto
    {
        public int id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public int parent_id { get; set; }
        public int project_id { get; set; }
        public bool delete_flag { get; set; } = false;
    }
    public class ProjectLevelResponseDTO
    {
        public int id { get; set; }
        public int parent_id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public int project_id { get; set; }
        public List<ProjectLevelResponseDTO> children { get; set; }
    }
}