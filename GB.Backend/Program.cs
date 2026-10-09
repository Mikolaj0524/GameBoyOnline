namespace GB.Backend
{
	public class Program
	{
		public static void Main(string[] args)
		{
			// Creates application builder
			var builder = WebApplication.CreateBuilder(args);


			// Registers controllers
			builder.Services.AddControllers();


			// Enables API endpoint discovery
			builder.Services.AddEndpointsApiExplorer();


			// Registers Swagger
			builder.Services.AddSwaggerGen();


			// Registers an in-memory cache
			builder.Services.AddMemoryCache();


			// Configures CORS
			builder.Services.AddCors(options =>
			{
				options.AddPolicy("Frontend", policy =>
				{
					policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
				});
			});


			// Builds the application
			var app = builder.Build();


			// Enables Swagger documentation
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}


			// Applies the configured CORS
			app.UseCors("Frontend");


			// Redirects HTTP requests to HTTPS
			//app.UseHttpsRedirection();


			// Enables auth middleware.
			app.UseAuthorization();


			// Maps controller routes
			app.MapControllers();


			// Starts web server
			app.Run();
		}
	}
}