
namespace CollaboratorService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Kestrel to use the PORT environment variable if available
            // In production (Cloud Run): uses $PORT from container environment
            // In development: uses port from appsettings.json or standard env vars, etc. if $PORT not set
            var environment = Environment.GetEnvironmentVariable("Environment");
            if (!string.IsNullOrEmpty(environment) && environment?.ToLower() != "development")
            {
                var port = Environment.GetEnvironmentVariable("PORT");
                // ensure port is valid (not blank/null, positive int)
                if (int.TryParse(port, out int parsedPort) && parsedPort > 0)
                {
                    builder.WebHost.UseUrls($"http://*:{port}");
                }
            }

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
