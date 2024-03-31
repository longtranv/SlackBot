using Microsoft.EntityFrameworkCore;
using SlackBot.Handlers;
using SlackBot.Models;
using SlackBot.Services;
using SlackNet.AspNetCore;
using SlackNet.Events;



var builder = WebApplication.CreateBuilder(args);

var slackSettings = builder.Configuration.GetSection("Slack").Get<SlackSettings>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<TodoContext>(option => option.UseSqlServer(builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING")));

builder.Services.AddSlackNet(c => c
    .UseApiToken(slackSettings?.ApiToken)
    .UseAppLevelToken(slackSettings?.AppLevelToken)
    .UseSigningSecret(slackSettings.SigningSecret)

    .RegisterEventHandler<MessageEvent, PingDemo>()
    .RegisterSlashCommandHandler<EchoDemo>(EchoDemo.SlashCommand)
    );

builder.Services.AddSingleton<EchoDemo>();
builder.Services.AddSingleton<SendSummary>();
builder.Services.AddHostedService<SendSummary>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "myPolicy", policy =>
    {
        policy.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("myPolicy");

app.UseAuthorization();

app.MapControllers();

app.UseSlackNet(c => c.UseSocketMode(true));

app.Run();

record SlackSettings
{
    public string ApiToken { get; init; } = string.Empty;
    public string AppLevelToken { get; init; } = string.Empty;
    public string SigningSecret { get; init; } = string.Empty;
}
