namespace DependencyInjectionWorkshop.Models
{
    public class NotifyDecorator : AuthenticationDecoratorBase
    {
        private readonly INotify _notify;

        public NotifyDecorator(IAuthenticationService authenticationService, INotify notify) : base(
            authenticationService)
        {
            _notify = notify;
        }

        private void Notify(string accountId)
        {
            _notify.Notify($"account:{accountId} try to login failed");
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var result = base.Verify(accountId, password, otp);
            if (result == false)
            {
                Notify(accountId);
            }

            return result;
        }
    }
}