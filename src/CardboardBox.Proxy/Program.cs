using CardboardBox.Proxy.Database;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
	   .AddSerilog()
	   .AddProxy();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseCors(c =>
{
	c.AllowAnyHeader()
	 .AllowAnyMethod()
	 .AllowAnyOrigin()
	 .WithExposedHeaders("Content-Disposition");
});

app.UseResponseCaching();

app.UseAuthorization();

app.MapControllers();

app.Run();
