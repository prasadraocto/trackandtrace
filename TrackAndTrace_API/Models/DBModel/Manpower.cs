namespace TrackAndTrace_API.Models.DBModel
{
    public class Manpower : CommonDBDto
    {
        public int designation_id { get; set; }
        public int engineer_id { get; set; }
        public int? charge_hand_id { get; set; }
        public int? gang_leader_id { get; set; }
        public int? subcontractor_id { get; set; }
        public decimal? rating { get; set; } = 0;
    }
}