using TrackAndTrace_API.Models.DBModel;

namespace TrackAndTrace_API.Models.RequestModel
{
    public class ManpowerDto : CommonParamDto
    {
        public int designation_id { get; set; }
        public int engineer_id { get; set; }
        public int? charge_hand_id { get; set; }
        public int? gang_leader_id { get; set; }
        public int? subcontractor_id { get; set; }
        public decimal? rating { get; set; } = 0;
        public List<Manpower_Project_Mapping> manpower_project_mapping { get; set; }
    }
}