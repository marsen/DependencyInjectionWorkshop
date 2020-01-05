namespace DependencyInjectionWorkshop.Models
{
    public interface INotification
    {
        /// <summary>
        /// Notifies the specified account identifier.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="message"></param>
        void Notify(string accountId, string message);
    }
}