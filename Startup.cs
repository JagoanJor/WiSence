using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using System;
using System.Diagnostics;
using System.Text.Json;

using API.Entities;
using API.Helpers;
using API.Services;

namespace API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Trace.Listeners.Add(new MyTraceListener());
            Trace.WriteLine("Starting API");

            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options => {
                options.AddDefaultPolicy(builder => {
                    builder
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin();
                });
            });

            services.AddControllers().AddNewtonsoftJson(options => {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "WiSence API",
                    Version = "v1",
                    Description = "WiSence API Documentation",
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                        new string[]{}
                    }
                });

            });

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IService<User>, UserService>();
            services.AddScoped<IService<Role>, RoleService>();
            services.AddScoped<IService<Vendor>, VendorService>();
            services.AddScoped<IService<Module>, ModuleService>();
            services.AddScoped<IService<Division>, DivisionService>();
            services.AddScoped<IService<Position>, PositionService>();
            services.AddScoped<IWifiService<Wifi>, WifiService>();
            services.AddScoped<IService<Calendar>, CalendarService>();
            services.AddScoped<IService<Shift>, ShiftService>();

            services.AddScoped<IDailyTaskService, DailyTaskService>();
            services.AddScoped<IAttendanceService, AttendanceService>();
            services.AddScoped<ICompanyService<Company>, CompanyService>();
            services.AddScoped<ICutiService, CutiService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IReportService, ReportService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseRouting();

            // global cors policy
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
            app.UseCors();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API API V1");

                // To serve SwaggerUI at application's root page, set the RoutePrefix property to an empty string.
                c.RoutePrefix = string.Empty;
            });

            app.UseMiddleware<JwtMiddleware>();

            app.UseEndpoints(x => x.MapControllers());
        }
    }
}

