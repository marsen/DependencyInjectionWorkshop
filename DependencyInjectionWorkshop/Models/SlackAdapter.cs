using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public interface INotification
    {
        /// <summary>
        /// Notifies the specified account identifier.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        void Notify(string accountId);
    }

    public class SlackAdapter : INotification
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