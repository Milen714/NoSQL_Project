
using MongoDB.Driver;
using NoSQL_Project.Repositories.Interfaces;
using NoSQL_Project.Repositories;
using NoSQL_Project.Services.Interfaces;
using NoSQL_Project.Services;
using NoSQL_Project.Models.PasswordResset;

namespace NoSQL_Project
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Load .env before building configuration so env vars are available
            DotNetEnv.Env.TraversePath().Load();

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddScoped<IUserRepository, UserRepository>();

            // 1) Register MongoClient as a SINGLETON (one shared instance for the whole app)
            // WHY: MongoClient is thread-safe and internally manages a connection pool.
            // Reusing one instance is fast and efficient. Creating many clients would waste resources.
            builder.Services.AddSingleton<IMongoClient>(sp =>
            {
                // Read the connection string from configuration (env var via .env)
                var conn = builder.Configuration["Mongo:ConnectionString"];
                if (string.IsNullOrWhiteSpace(conn))
                    throw new InvalidOperationException("Mongo:ConnectionString is not configured. Did you set it in .env?");

                // Optional: tweak settings (timeouts, etc.)
                var settings = MongoClientSettings.FromConnectionString(conn);
                // settings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);

                return new MongoClient(settings);
            });

            // 2) Register IMongoDatabase as SCOPED (new per HTTP request)
            // WHY: Fits the ASP.NET request lifecycle and keeps each request cleanly separated.
            builder.Services.AddScoped(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();

                var dbName = builder.Configuration["Mongo:Database"]; // from appsettings.json
                if (string.IsNullOrWhiteSpace(dbName))
                    throw new InvalidOperationException("Mongo:Database is not configured in appsettings.json.");

                return client.GetDatabase(dbName);
            });
            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ILocationRepository, LocationRepository>();
            builder.Services.AddScoped<ILocationService, LocationService>();
            builder.Services.AddScoped<IIncidentRepository, IncidentRepository>();
            builder.Services.AddScoped<IIncidentService, IncidentService>();

            // Search functionality
            builder.Services.AddScoped<IIncidentSearchRepository, IncidentSearchRepository>();
            builder.Services.AddScoped<IIncidentSearchService, IncidentSearchService>();

            // Sort functionality 
            builder.Services.AddScoped<IIncidentSortRepository, IncidentSortRepository>();
            builder.Services.AddScoped<IIncidentSortService, IncidentSortService>();

            //Session
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60); // Set the timeout to 60 minutes
                options.Cookie.HttpOnly = true; // Make the cookie HTTP-only   
                options.Cookie.IsEssential = true; // Make the session cookie essential
            });

            // Email service
            builder.Services.Configure<EmailSettings>(options =>
            {
                options.SmtpServer = Environment.GetEnvironmentVariable("SMTP_SERVER");
                options.SmtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");
                options.SenderName = Environment.GetEnvironmentVariable("SMTP_SENDER_NAME");
                options.SenderEmail = Environment.GetEnvironmentVariable("SMTP_SENDER_EMAIL");
                options.Username = Environment.GetEnvironmentVariable("SMTP_USERNAME");
                options.Password = Environment.GetEnvironmentVariable("SMTP_PASSWORD");
            });
            builder.Services.AddTransient<IEmailSenderService, GmailSenderService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Login}/{id?}");

            app.Run();
        }
    }
}


