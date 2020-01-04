using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly INotify _slackAdapter;
        private readonly FailCounter _failCounter;
        private readonly OtpAdapter _otpAdapter;
        private readonly NLogAdapter _nLogAdapter;

        public AuthenticationService(IProfile profile, IHash hash, INotify slackAdapter, FailCounter failCounter, OtpAdapter otpAdapter, NLogAdapter nLogAdapter)
        {
            _profile = profile;
            _hash = hash;
            _slackAdapter = slackAdapter;
            _failCounter = failCounter;
            _otpAdapter = otpAdapter;
            _nLogAdapter = nLogAdapter;
        }

        public AuthenticationService()
        {
            _profile = new ProfileDao();
            _hash = new Sha256Adapter();
            _slackAdapter = new SlackAdapter();
            _failCounter = new FailCounter();
            _otpAdapter = new OtpAdapter();
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

            var currentOtp = _otpAdapter.GetOtp(accountId);

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
                _slackAdapter.Notify(accountId);
                    return false;
            }
        }
    }
}