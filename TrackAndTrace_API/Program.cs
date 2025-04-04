using TrackAndTrace_API.Helpers;
using TrackAndTrace_API.Interface;
using TrackAndTrace_API.Models;
using TrackAndTrace_API.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Mvc.Filters;
using TrackAndTrace_API.HostedService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader()
    );
});

// Add DbContext if you use it
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Track And Trace API", Version = "v1" });
    options.SwaggerDoc("v2", new OpenApiInfo { Title = "Track And Trace API", Version = "v2" });

    // Set up the Bearer Authentication for Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Hangfire configuration
builder.Services.AddHangfire(config =>
{
    config.UseMemoryStorage();
});
builder.Services.AddHangfireServer();

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]))
        };
    });

builder.Services.AddScoped<JwtHelper>(); // Register the JwtHelper for dependency injection

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<ICommonRepository, CommonRepository>();
builder.Services.AddScoped<ILoginRepository, LoginRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IDesignationRepository, DesignationRepository>();
builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ISub_CategoryRepository, Sub_CategoryRepository>();
builder.Services.AddScoped<IUOMRepository, UOMRepository>();
builder.Services.AddScoped<IDifferentiatorRepository, DifferentiatorRepository>();
builder.Services.AddScoped<ISpecificationRepository, SpecificationRepository>();
builder.Services.AddScoped<IMachineryRepository, MachineryRepository>();
builder.Services.AddScoped<IInjuryRepository, InjuryRepository>();
builder.Services.AddScoped<IShiftRepository, ShiftRepository>();
builder.Services.AddScoped<IWeatherRepository, WeatherRepository>();
builder.Services.AddScoped<ILabour_TypeRepository, Labour_TypeRepository>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
builder.Services.AddScoped<ISubcontractorRepository, SubcontractorRepository>();
builder.Services.AddScoped<IConfigurationRepository, ConfigurationRepository>();

builder.Services.AddScoped<IMaterialRepository, MaterialRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IWorkflowRepository, WorkflowRepository>();
builder.Services.AddScoped<IActivityRepository, ActivityRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ISub_TaskRepository, Sub_TaskRepository>();
builder.Services.AddScoped<IProjectMaterialRepository, ProjectMaterialRepository>();
builder.Services.AddScoped<IProjectLevelRepository, ProjectLevelRepository>();
builder.Services.AddScoped<IManpowerRepository, ManpowerRepository>();
builder.Services.AddScoped<IDailyActivityRepository, DailyActivityRepository>();
builder.Services.AddScoped<IUserAttendanceRepository, UserAttendanceRepository>();
builder.Services.AddScoped<ISpaceManagementRepository, SpaceManagementRepository>();
builder.Services.AddScoped<IMeetingRepository, MeetingRepository>();
builder.Services.AddScoped<IActivityMilestoneRepository, ActivityMilestoneRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IIndentRepository, IndentRepository>();

var app = builder.Build();

app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Track And Trace API v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "Track And Trace API v2");

        options.DefaultModelsExpandDepth(-1); // Hide models section (optional)
        options.RoutePrefix = string.Empty; // Swagger UI at root
    });
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Track And Trace API v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "Track And Trace API v2");
        options.DefaultModelsExpandDepth(-1);  // Optional: Hide models section
        options.RoutePrefix = string.Empty;   // Swagger UI at root
    });
}

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DashboardTitle = "Sample Jobs",
    Authorization = new[]
    {
        new  MyAuthorizationFilter("admin")
    }
});

app.MapHangfireDashboard();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
