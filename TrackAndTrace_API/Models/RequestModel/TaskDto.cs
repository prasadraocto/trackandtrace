using TrackAndTrace_API.Models.DBModel;

namespace TrackAndTrace_API.Models.RequestModel
{
    public class TaskDto : CommonParamDto
    {
        public int activity_id { get; set; }
        public int uom_id { get; set; }
        public List<Task_Project_Mapping> project_mapping { get; set; }
    }
}