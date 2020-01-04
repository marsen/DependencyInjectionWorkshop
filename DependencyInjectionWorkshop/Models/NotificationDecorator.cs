namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator : IAuthentication
    {
        private INotification _notification;
        private IAuthentication _authenticationService;

        public NotificationDecorator(IAuthentication authenticationService, INotification notification)
        {
            _authenticationService = authenticationService;
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