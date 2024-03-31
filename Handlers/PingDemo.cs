using SlackNet;
using SlackNet.Events;
using SlackNet.Interaction;
using SlackNet.Interaction.Experimental;
using SlackNet.WebApi;
using SlackAPI.WebSocketMessages;

namespace SlackBot.Handlers
{
    public class PingDemo : IEventHandler<MessageEvent>
    {
        private readonly ISlackApiClient _slackApiClient;
        private const string Trigger = "ping";

        public PingDemo(ISlackApiClient slackApiClient)
        {
            _slackApiClient = slackApiClient;
        }

        public async Task Handle(MessageEvent slackEvent)
        {
            if (slackEvent.Text?.Contains(Trigger, StringComparison.OrdinalIgnoreCase) == true)
            {
                await _slackApiClient.Chat.PostMessage(new SlackNet.WebApi.Message()
                {
                    Text = "pong-chan",
                    Channel = slackEvent.Channel,
                });
            }
        }
    }
}
