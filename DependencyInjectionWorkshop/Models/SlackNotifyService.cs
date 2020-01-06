using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class SlackNotifyService
    {
        public void Notify(string message)
        {
            //notify
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(messageResponse => { }, "my channel", message, "my bot name");
        }
    }
}