using Microsoft.EntityFrameworkCore;
using Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// things we use inside our application

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DataContext>(opt =>
{
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddCors(opt => {
    opt.AddPolicy("CorsPolicy", policy =>
    {
        //policy.WithOrigins("http://localhost:3000/").AllowAnyHeader().AllowAnyMethod();
        //policy.WithOrigins("*").AllowAnyHeader().AllowAnyMethod();
        policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:3000","http://localhost:5000");
    });
});


// Build App
var app = builder.Build();



// Configure the HTTP request pipeline. 
// Middleware - things that can do something with http request on its way in or out
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsPolicy"); // order of middleware is important
//app.UseCors(policy => policy.WithOrigins("http://localhost:3000/").AllowAnyMethod().AllowAnyHeader());

app.UseAuthorization();

app.MapControllers();

// Database Work
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

try
{
    var context = services.GetRequiredService<DataContext>();
    await context.Database.MigrateAsync();
    await Seed.SeedData(context);
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occured during migration.");
}

app.Run();
