namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationDecoratorBase : IAuthenticationService
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationDecoratorBase(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public virtual bool Verify(string accountId, string password, string otp)
        {
            var result = _authenticationService.Verify(accountId, password, otp);

            return result;
        }
    }
}