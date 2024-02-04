using Google.Apis.Sheets.v4;
using Google_Sheets.Data;
using Google_Sheets.Services;
using Google_Sheets.Services.Google_Sheets.Services;
using Google_Sheets.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Google_Sheets
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddControllersWithViews();
            builder.Services.AddTransient<ISheetsService, MySheetsService>();
            //builder.Services.AddTransient<GoogleSheetsAPIService>();

            builder.Services.AddSingleton<GoogleSheetsAPIService>(serviceProvider =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var serviceAccountFilePath = configuration["GoogleSheets:ServiceAccountFilePath"];
                if (string.IsNullOrEmpty(serviceAccountFilePath) || !File.Exists(serviceAccountFilePath))
                {
                    throw new FileNotFoundException("The service account file was not found.", serviceAccountFilePath);
                }
                return new GoogleSheetsAPIService(serviceAccountFilePath);
            });



            builder.Services.AddTransient<SheetsService>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            //app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            
            app.UseRouting();

            app.MapControllers();
            app.MapRazorPages();

            app.Run();
        }
    }
}