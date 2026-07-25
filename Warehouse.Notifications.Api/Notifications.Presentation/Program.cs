using Microsoft.EntityFrameworkCore;
using Notifications.Application.Notification;
using Notifications.Application.Notification.Queries.ListNotifications;
using Notifications.Domain.Notification;
using Notifications.Infrastructure.Messaging;
using Notifications.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ListNotificationsQuery).Assembly));
builder.Services.AddAutoMapper(_ => { }, typeof(NotificationMappingProfile).Assembly);

builder.Services.AddHostedService<WarehouseEventsConsumer>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();