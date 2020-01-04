namespace DependencyInjectionWorkshop.Models.Decorator
{
    public class AuthenticationDecoratorBase : IAuthentication
    {
        private readonly IAuthentication _authenticationService;

        public AuthenticationDecoratorBase(IAuthentication authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public virtual bool Verify(string accountId, string password, string otp)
        {
            return _authenticationService.Verify(accountId, password, otp);
        }
    }
}