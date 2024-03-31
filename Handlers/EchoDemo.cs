using SlackBot.Services;
using SlackNet;
using SlackNet.Interaction;

namespace SlackBot.Handlers
{
    public class EchoDemo : ISlashCommandHandler
    {
        public const string SlashCommand = "/diemdanh";
        private readonly ISlackApiClient _slackApiClient;
        public DateTime formSentTime;
        public string channel;
        private readonly SendSummary _sendSummary;
        public double deadline = 5;
        private readonly string FormLink = "https://myform-inky.vercel.app/";

        public EchoDemo(ISlackApiClient slackApiClient, SendSummary sendSummary)
        {
            _slackApiClient = slackApiClient;
            _sendSummary = sendSummary;
        }
        public async Task<SlashCommandResponse> Handle(SlashCommand command)
        {
            if (command.UserName == "gialongfp" && command.Command == SlashCommand)
            {

                string messageResponse;
                if (double.TryParse(command.Text, out double doubleValue))
                {
                    if(doubleValue == 0) { messageResponse = "can't set 0 minute. Using default deadline which is 5 minutes"; }
                    else { 
                        deadline = doubleValue;
                        messageResponse = "Success!";
                    }
                    
                }
                else
                {
                    messageResponse = "wrong number format. Using default deadline which is 5 minutes";
                }
                formSentTime = DateTime.UtcNow;
                channel = command.ChannelName;
                _sendSummary.Start(formSentTime, channel, deadline);
                await _slackApiClient.Chat.PostMessage(new SlackNet.WebApi.Message()
                {
                    Text = $"checking presence time begins, deadline is {deadline} minutes\nplease access this link to submit the form: <{FormLink}|link>",
                    Channel = command.ChannelName,
                });
                return new SlashCommandResponse() { Message = new SlackNet.WebApi.Message() { Text = messageResponse } };
            }
            return new SlashCommandResponse() { Message = new SlackNet.WebApi.Message() { Text = "You are not admin!" } };
        }
    }
}
