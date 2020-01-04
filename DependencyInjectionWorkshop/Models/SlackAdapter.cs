using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{

    public class SlackAdapter
    {
        public SlackAdapter()
        {
        }

        /// <summary>
        /// Notifies the specified account identifier.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        public void Notify(string accountId)
        {
            //notify
            var message = $"account:{accountId} try to login failed";
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(messageResponse => { }, "my channel", message, "my bot name");
        }
    }
}