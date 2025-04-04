namespace TrackAndTrace_API.Models.ResponseModel
{
    public class IndentByIdResponse
    {
        public int id { get; set; }
        public string indent_type { get; set; }
        public string indent_no { get; set; }
        public string indent_date { get; set; }
        public int project_id { get; set; }
        public string project_name { get; set; }
        public int raised_by_id { get; set; }
        public string raised_by_name { get; set; }
        public string approval_status { get; set; }
        public List<IndentDetails> indent_details { get; set; }
    }
    public class IndentDetails
    {
        public int id { get; set; }
        public string material_type { get; set; }
        public int material_id { get; set; }
        public string material_name { get; set; }
        public string material_description { get; set; }
        public decimal quantity { get; set; }
        public string uom { get; set; }
        public int? lead_days { get; set; }
        public string? delivery_date { get; set; }
        public decimal? supply_cost { get; set; }
        public string? remarks { get; set; }
        public string? material_specification { get; set; }
    }
}
