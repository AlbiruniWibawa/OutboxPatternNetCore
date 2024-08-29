using Microsoft.EntityFrameworkCore;
using UserAppService.Data;
using UserAppService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<UserServiceContext>(options => options.UseSqlite(@"Data Source=user.db"));

builder.Services.AddSingleton<IntegrationEventSenderService>();
builder.Services.AddHostedService<IntegrationEventSenderService>(provider => provider.GetService<IntegrationEventSenderService>());
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Add this part
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<UserServiceContext>();
        dbContext.Database.EnsureCreated();
    }
    // ------- //
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
