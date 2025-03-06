using Microsoft.EntityFrameworkCore;
using Zephyr.Db;
using Zephyr.MetarUpdate;
using Zephyr.Scheduler;

namespace Zephyr
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            
            builder.Services.AddDbContext<ZephyrDbContext>(options =>
                options.UseSqlite("Data Source=zephyr.db"));
            
            builder.Services.AddScoped<IMetarStorageService, MetarStorageService>();
            builder.Services.AddScoped<FetchRemoteMetar>();

            builder.Services.Configure<BackgroundServiceOptions>(
                builder.Configuration.GetSection("MetarBackgroundService"));
            
            builder.Services.Configure<MetarUpdateOptions>(
                builder.Configuration.GetSection("MetarUpdate"));

            builder.Services.AddHostedService<MetarBackgroundServce>();

            builder.Services.AddHttpClient("MetarClient", client =>
            {
                // You can configure default headers, timeout etc. here
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            var app = builder.Build();

            // Ensure database is created
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ZephyrDbContext>();
                dbContext.Database.Migrate();
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.MapControllers();

            app.Run();
        }
    }
}
