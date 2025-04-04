using TrackAndTrace_API.Models.DBModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace TrackAndTrace_API.Models
{
    public class ApplicationDbContext : DbContext
    {
        // DbSet properties for each entity
        public DbSet<Roles> Roles { get; set; }
        public DbSet<Company> Company { get; set; }
        public DbSet<Designation> Designation { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<Menu> Menu { get; set; }
        public DbSet<Page> Page { get; set; }
        public DbSet<MenuPageMapping> Menu_Page_Mapping { get; set; }
        public DbSet<CompanyRoleMenuPageMapping> Company_Role_Menu_Page_Mapping { get; set; }
        public DbSet<Bulk_Import_Details> Bulk_Import_Details { get; set; }
        //Master Models
        public DbSet<Brand> Brand { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Sub_Category> Sub_Category { get; set; }
        public DbSet<UOM> UOM { get; set; }
        public DbSet<Machinery> Machinery { get; set; }
        public DbSet<Injury> Injury { get; set; }
        public DbSet<Shift> Shift { get; set; }
        public DbSet<Weather> Weather { get; set; }
        public DbSet<Labour_Type> Labour_Type { get; set; }
        public DbSet<Warehouse> Warehouse { get; set; }
        public DbSet<Subcontractor> Subcontractor { get; set; }

        //Transaction Models
        public DbSet<Material> Material { get; set; }
        public DbSet<Differentiator> Differentiator { get; set; }
        public DbSet<Differentiator_Mapping> Differentiator_Mapping { get; set; }
        public DbSet<Specification> Specification { get; set; }
        public DbSet<Specification_Differentiator_Mapping> Specification_Differentiator_Mapping { get; set; }
        public DbSet<Material_Brand_Mapping> Material_Brand_Mapping { get; set; }
        public DbSet<Project> Project { get; set; }
        public DbSet<Project_User_Mapping> Project_User_Mapping { get; set; }
        public DbSet<Work_Flow> Work_Flow { get; set; }
        public DbSet<Work_Flow_Project_User_Mapping> Work_Flow_Project_User_Mapping { get; set; }
        public DbSet<Activity> Activity { get; set; }
        public DbSet<Activity_Project_Mapping> Activity_Project_Mapping { get; set; }
        public DbSet<TrackAndTrace_API.Models.DBModel.Tasks> Task { get; set; }
        public DbSet<Task_Project_Mapping> Task_Project_Mapping { get; set; }
        public DbSet<Sub_Task> Sub_Task { get; set; }
        public DbSet<Sub_Task_Project_Mapping> Sub_Task_Project_Mapping { get; set; }
        public DbSet<Trx_Work_Flow_Approval_Status> Trx_Work_Flow_Approval_Status { get; set; }
        public DbSet<Project_Level_Mapping> Project_Level_Mapping { get; set; }
        public DbSet<Manpower> Manpower { get; set; }
        public DbSet<Manpower_Project_Mapping> Manpower_Project_Mapping { get; set; }
        public DbSet<Trx_Daily_Activity_Item> Trx_Daily_Activity_Item { get; set; }
        public DbSet<Trx_Daily_Activity_Details> Trx_Daily_Activity_Details { get; set; }
        public DbSet<Trx_Daily_Activity_Manpower> Trx_Daily_Activity_Manpower { get; set; }
        public DbSet<Trx_Daily_Activity_Material> Trx_Daily_Activity_Material { get; set; }
        public DbSet<Trx_Daily_Activity_Machinery> Trx_Daily_Activity_Machinery { get; set; }
        public DbSet<User_Attendance> User_Attendance { get; set; }
        public DbSet<Space_Management> Space_Management { get; set; }
        public DbSet<Meeting> Meeting { get; set; }
        public DbSet<Meeting_Attendee_Detail> Meeting_Attendee_Detail { get; set; }
        public DbSet<Meeting_Assigned_Task> Meeting_Assigned_Task { get; set; }
        public DbSet<Activity_Milestone> Activity_Milestone { get; set; }
        public DbSet<Activity_Milestone_Mapping> Activity_Milestone_Mapping { get; set; }
        public DbSet<Indent> Indent { get; set; }
        public DbSet<Indent_Material> Indent_Material { get; set; }
        public DbSet<Indent_Material_Differentiator> Indent_Material_Differentiator { get; set; }

        // Constructor to pass options to the base class
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            //Nothing
        }

        // Optionally, you can override the OnModelCreating method if you need custom configurations
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}