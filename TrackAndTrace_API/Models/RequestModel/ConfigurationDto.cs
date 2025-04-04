namespace TrackAndTrace_API.Models.RequestModel
{
    public class MenuDto
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }
    public class PageDto
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string url { get; set; }
    }
    public class MenuPageMappingDto
    {
        public int id { get; set; }
        public int menu_id { get; set; }
        public List<PageMappingLst> pages { get; set; }
    }
    public class PageMappingLst
    {
        public int page_id { get; set; }
        public int mapping_order { get; set; }
    }
    public class CompanyRoleMenuPageMappingDto
    {
        public int id { get; set; }
        public int company_id { get; set; }
        public int role_id { get; set; }
        public int menu_id { get; set; }
        public List<PageMappingLst> pages { get; set; }
    }
}
