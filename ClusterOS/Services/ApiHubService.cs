using Hub.Application;
using Hub.Infrastructure.Repositories;
using Hub.Infrastructure;
using Hub.Presentation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using Windows.Storage;
using Hub.Infrastructure.Services;

namespace ClusterOS.Services
{
    public class ApiHubService
    {
        public ApiHubService(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            string localFolderPath = ApplicationData.Current.LocalFolder.Path;
            string dbPath = Path.Combine(localFolderPath, "hub.db");
            services.AddSignalR();
            services.AddDbContext<HubDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));
            services.AddScoped<ITabRepository, TabRepository>();
            services.AddScoped<IApplicationRepository, ApplicationRepository>();
            services.AddScoped<ITrackDataRepository, TrackDataRepository>();
            services.AddScoped<ITaskItemRepository, TaskItemRepository>();
            services.AddScoped<IJournalRepository, JournalRepository>();
            services.AddCors(options =>
            {
                options.AddPolicy("BrowserPolicy", policy =>
                {
                    policy.SetIsOriginAllowed(origin => origin.StartsWith("moz-extension://"))
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            services.AddHostedService<WindowsWindowTrackingService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<HubDbContext>();
                dbContext.Database.Migrate();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("BrowserPolicy");
            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<TabFocus>("/tabfocused");
                endpoints.MapHub<AppFocusHub>("/appfocused");
                endpoints.MapTrackDataEndpoints();
                endpoints.MapJournalEndpoints();
                endpoints.MapTaskEndpoints();
            });
        }
    }
}
