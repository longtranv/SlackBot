using Microsoft.EntityFrameworkCore;
using SlackAPI.WebSocketMessages;
using SlackBot.Models;
using SlackNet;

namespace SlackBot.Services
{
    public class SendSummary : IHostedService, IDisposable
    {
        private readonly ILogger<SendSummary> _logger;
        private Timer _timer;
        private bool _Run = false;
        private readonly ISlackApiClient _slack;
        private DateTime _formSentTime;
        private string Channel;
        private double _deadline;
        private readonly IServiceScopeFactory _scopeFactory;
        public SendSummary(ILogger<SendSummary> logger, ISlackApiClient slackApiClient, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _slack = slackApiClient;
            _scopeFactory = scopeFactory;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Background Service is starting");

            if (!_Run)
                return Task.CompletedTask;

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            if (!_Run)
            {
                StopAsync(CancellationToken.None).GetAwaiter().GetResult();
                return;
            }

            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TodoContext>();
                var records =
                from t in dbContext.AttendenceRecords
                join x in dbContext.Members on t.MemberId equals x.Id
                where t.Date.Date == DateTime.UtcNow.Date
                select new { Name = x.Name, state = t.IsPresent };

                var items = records.ToList();

                TimeSpan elapsedTime = DateTime.UtcNow - _formSentTime;
                if (elapsedTime.TotalMinutes >= _deadline)
                {
                    _slack.Chat.PostMessage(new SlackNet.WebApi.Message()
                    {
                        Text = "----------Report Summary----------",
                        Channel = Channel,
                    });
                    foreach (var item in items)
                    {
                        dynamic dynamic = item as dynamic;
                       _slack.Chat.PostMessage(new SlackNet.WebApi.Message()
                        {
                            Text = $"------------------\n{dynamic.Name}: presenced\n------------------",
                            Channel = Channel,
                        });
                    }
                    _Run = false;
                    StopAsync(CancellationToken.None).GetAwaiter().GetResult();
                }

            }
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Background service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer.Dispose();
        }

        public void Start(DateTime formSentTime, string channel, double deadline)
        {
            _Run = true;
            _formSentTime = formSentTime;
            Channel = channel;
            _deadline = deadline;
            StartAsync(CancellationToken.None).GetAwaiter().GetResult();
        }
    }
}
