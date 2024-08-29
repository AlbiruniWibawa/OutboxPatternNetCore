using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using UserPostAppService.Data;
using System.Text;
using Newtonsoft.Json.Linq;
using UserPostAppService.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<PostServiceContext>(options => options.UseSqlite(@"Data Source=post.db"));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<PostServiceContext>();
        dbContext.Database.EnsureCreated();
    }
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

ListenForIntegrationEvents(app);

app.Run();

static void ListenForIntegrationEvents(IHost host)
{
    var factory = new ConnectionFactory();
    var connection = factory.CreateConnection();
    var channel = connection.CreateModel();
    var consumer = new EventingBasicConsumer(channel);

    consumer.Received += (model, ea) =>
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        Console.WriteLine(" [x] Received {0}", message);
        var data = JObject.Parse(message);
        var type = ea.RoutingKey;

        using var localScope = host.Services.CreateScope();
        var localDbContext = localScope.ServiceProvider.GetRequiredService<PostServiceContext>();

        if (type == "user.add")
        {
            localDbContext.User.Add(new User()
            {
                ID = data["id"].Value<int>(),
                Name = data["name"].Value<string>()
            });
            localDbContext.SaveChanges();
        }
        else if (type == "user.update")
        {
            var user = localDbContext.User.First(a => a.ID == data["id"].Value<int>());
            user.Name = data["newname"].Value<string>();
            localDbContext.SaveChanges();
        }
        channel.BasicAck(ea.DeliveryTag, false);
    };

    channel.BasicConsume(queue: "user.postservice",
                         autoAck: false,
                         consumer: consumer);
}
