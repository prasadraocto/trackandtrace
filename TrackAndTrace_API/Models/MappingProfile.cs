using TrackAndTrace_API.Models.DBModel;
using TrackAndTrace_API.Models.RequestModel;
using AutoMapper;

namespace TrackAndTrace_API.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Company, CompanyDto>().ReverseMap();
            CreateMap<Users, UsersDto>().ReverseMap();
            CreateMap<Designation, DesignationDto>().ReverseMap();
            CreateMap<Brand, BrandDto>().ReverseMap();
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<Sub_Category, Sub_CategoryDto>().ReverseMap();
            CreateMap<UOM, UOMDto>().ReverseMap();
            CreateMap<Menu, MenuDto>().ReverseMap();
            CreateMap<Page, PageDto>().ReverseMap();
            CreateMap<Differentiator, DifferentiatorDto>().ReverseMap();
            CreateMap<Specification, SpecificationDto>().ReverseMap();
            CreateMap<Machinery, MachineryDto>().ReverseMap();
            CreateMap<Injury, InjuryDto>().ReverseMap();
            CreateMap<Shift, ShiftDto>().ReverseMap();
            CreateMap<Weather, WeatherDto>().ReverseMap();
            CreateMap<Labour_Type, Labour_TypeDto>().ReverseMap();
            CreateMap<Warehouse, WarehouseDto>().ReverseMap();
            CreateMap<Subcontractor, SubcontractorDto>().ReverseMap();
            CreateMap<MenuPageMapping, MenuPageMappingDto>().ReverseMap();

            CreateMap<Material, MaterialDto>().ReverseMap();
            CreateMap<Project, ProjectDto>().ReverseMap();
            CreateMap<Activity, ActivityDto>().ReverseMap();
            CreateMap<Tasks, TaskDto>().ReverseMap();
            CreateMap<Sub_Task, Sub_TaskDto>().ReverseMap();
            CreateMap<Sub_Task_Project_Mapping, Sub_Task_Project_MappingDto>().ReverseMap();
            CreateMap<Project_Level_Mapping, ProjectLevelDto>().ReverseMap();
            CreateMap<Manpower, ManpowerDto>().ReverseMap();
            CreateMap<Trx_Daily_Activity_Details, DailyActivityDto>().ReverseMap();
            CreateMap<User_Attendance, UserAttendanceDto>().ReverseMap();
            CreateMap<Space_Management, SpaceManagementDto>().ReverseMap();
            CreateMap<Meeting, MeetingDto>().ReverseMap();
            CreateMap<Meeting_Assigned_Task, MeetingAssignedTaskDto>().ReverseMap();
            CreateMap<Activity_Milestone, ActivityMilestoneDto>().ReverseMap();
        }
    }
}