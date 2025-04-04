using System;

namespace TrackAndTrace_API.Models.DBModel
{
    public class Page
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string url { get; set; }
        public DateTime created_date { get; set; }
        public DateTime? updated_date { get; set; }
    }
    public class Menu
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
        public DateTime created_date { get; set; }
        public DateTime? updated_date { get; set; }
    }
    public class MenuPageMapping
    {
        public int id { get; set; }
        public int menu_id { get; set; }
        public int page_id { get; set; }
        public int mapping_order { get; set; }
        public DateTime created_date { get; set; }
    }
    public class CompanyRoleMenuPageMapping
    {
        public int id { get; set; }
        public int company_id { get; set; }
        public int role_id { get; set; }
        public int menu_id { get; set; }
        public int page_id { get; set; }
        public int mapping_order { get; set; }
        public DateTime created_date { get; set; }
    }
}