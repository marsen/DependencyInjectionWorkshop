using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly INotification _notification;
        private readonly IFailCounter _failCounter;
        private readonly IOtpService _otpService;
        private readonly ILog _nLogAdapter;

        public AuthenticationService(IProfile profile, IHash hash, INotification notification, IFailCounter failCounter, IOtpService otpService, ILog nLogAdapter)
        {
            _profile = profile;
            _hash = hash;
            _notification = notification;
            _failCounter = failCounter;
            _otpService = otpService;
            _nLogAdapter = nLogAdapter;
        }

        public AuthenticationService()
        {
            _profile = new ProfileDao();
            _hash = new Sha256Adapter();
            _notification = new SlackAdapter();
            _failCounter = new FailCounter();
            _otpService = new OtpService();
            _nLogAdapter = new NLogAdapter();
        }

        /// <summary>
        /// Verifies the specified account identifier.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="password">The password.</param>
        /// <param name="otp">The otp.</param>
        /// <returns></returns>
        /// <exception cref="DependencyInjectionWorkshop.Models.FailedTooManyTimesException"></exception>
        public bool Verify(string accountId, string password, string otp)
        {
            var isLocked = _failCounter.IsLocked(accountId);
            if (isLocked)
            {
                throw new FailedTooManyTimesException(){AccountId = accountId};
            }

            var passwordFromDb = _profile.Password(accountId);

            var hashedPassword = _hash.ComputeHash(password);

            var currentOtp = _otpService.GetOtp(accountId);

            if (currentOtp == otp && hashedPassword == passwordFromDb)
            {
                _failCounter.Reset(accountId);
                return true;
            }
            else
            {
                _failCounter.Add(accountId);

                var failedCount = _failCounter.Get(accountId);
                _nLogAdapter.Info(accountId, failedCount);
                _notification.Notify(accountId);
                    return false;
            }
        }
    }
}