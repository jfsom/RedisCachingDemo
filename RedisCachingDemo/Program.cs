using Microsoft.EntityFrameworkCore;
using RedisCachingDemo.Data;
using StackExchange.Redis;

namespace RedisCachingDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Add services to the container.

            // Configure the application's DbContext to use SQL Server
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                // Specify the connection string for the SQL Server database
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add Redis distributed caching service
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                // Specify the Redis server configuration
                // The "Configuration" value is fetched from the appsettings.json file
                // Example format: "localhost:6379" (host and port of the Redis server)
                options.Configuration = builder.Configuration["RedisCacheOptions:Configuration"];

                // Set an instance name for the Redis cache entries
                // This acts as a prefix for keys created by this application in Redis
                options.InstanceName = builder.Configuration["RedisCacheOptions:InstanceName"];
            });

            // Register the Redis connection multiplexer as a singleton service
            // This allows the application to interact directly with Redis for advanced scenarios
            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
                // Establish a connection to the Redis server using the configuration from appsettings.json
                ConnectionMultiplexer.Connect(builder.Configuration["RedisCacheOptions:Configuration"]));

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}