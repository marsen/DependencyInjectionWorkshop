using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class SlackAdapter : INotification
    {
        public SlackAdapter()
        {
        }

        /// <summary>
        /// Notifies the specified account identifier.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="message"></param>
        public void Notify(string accountId, string message)
        {
            //notify
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(messageResponse => { }, "my channel", message, "my bot name");
        }
    }
}