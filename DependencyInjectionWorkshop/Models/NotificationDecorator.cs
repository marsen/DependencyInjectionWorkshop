namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator : IAuthentication
    {
        private readonly INotification _notification;
        private readonly IAuthentication _authenticationService;

        public NotificationDecorator(IAuthentication authenticationService, INotification notification)
        {
            _authenticationService = authenticationService;
            _notification = notification;
        }

        private void Notify(string accountId)
        {
            _notification.Notify(accountId, $"account:{accountId} try to login failed");
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var isValid = _authenticationService.Verify(accountId, password, otp);
            if (!isValid)
            {
                Notify(accountId);
            }

            return isValid;
        }
    }
}