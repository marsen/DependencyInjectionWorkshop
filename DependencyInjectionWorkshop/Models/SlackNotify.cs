using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public interface INotify
    {
        void Notify(string message);
    }

    public class SlackNotify : INotify
    {
        public void Notify(string message)
        {
            //notify
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(messageResponse => { }, "my channel", message, "my bot name");
        }
    }
}