using DependencyInjectionWorkshop.Models.Adapter;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService : IAuthentication
    {
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOtpService _otpService;

        public AuthenticationService(IProfile profile, IHash hash, IOtpService otpService)
        {
            _profile = profile;
            _hash = hash;
            _otpService = otpService;
        }

        public AuthenticationService()
        {
            _profile = new ProfileDao();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
        }

        /// <summary>
        /// Verifies the specified account identifier.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="password">The password.</param>
        /// <param name="otp">The otp.</param>
        /// <returns></returns>
        /// <exception cref="FailedTooManyTimesException"></exception>
        public bool Verify(string accountId, string password, string otp)
        {
            return CheckOtp(otp, accountId) && CheckPassword(accountId, password);
        }

        private bool CheckPassword(string accountId, string password)
        {
            return _hash.ComputeHash(password) == _profile.Password(accountId);
        }

        private bool CheckOtp(string otp, string accountId)
        {
            return _otpService.GetOtp(accountId) == otp;
        }
    }
}