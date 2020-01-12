namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService : IAuthenticationService
    {
        public AuthenticationService(IProfile profile, IHash hash, IOtpService otpService)
        {
            _profile = profile;
            _hash = hash;
            _otpService = otpService;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationService" /> class.
        /// </summary>
        public AuthenticationService()
        {
            _profile = new Profile();
            _hash = new Sha256Hash();
            _otpService = new OtpService();
        }

        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOtpService _otpService;

        public bool Verify(string accountId, string password, string otp)
        {
            var passwordFromDb = _profile.GetPassword(accountId);

            var hashedPassword = _hash.Hash(password);

            var currentOtp = _otpService.CurrentOtp(accountId);

            //compare
            if (passwordFromDb == hashedPassword && currentOtp == otp)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}