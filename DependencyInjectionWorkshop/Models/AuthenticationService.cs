using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly ProfileDao _profileDao;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly SlackAdapter _slackAdapter;
        private readonly FailCounter _failCounter;
        private readonly OtpAdapter _otpAdapter;
        private readonly NLogAdapter _nLogAdapter;

        public AuthenticationService()
        {
            _profileDao = new ProfileDao();
            _sha256Adapter = new Sha256Adapter();
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
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
            
            var isLocked = _failCounter.IsLocked(accountId, httpClient);
            if (isLocked)
            {
                throw new FailedTooManyTimesException(){AccountId = accountId};
            }

            var passwordFromDb = _profileDao.PasswordFromDb(accountId);

            var hashedPassword = _sha256Adapter.HashedPassword(password);

            var currentOtp = _otpAdapter.GetOtp(accountId, httpClient);

            if (currentOtp == otp && hashedPassword == passwordFromDb)
            {
                _failCounter.Reset(accountId, httpClient);
                return true;
            }
            else
            {
                _failCounter.Add(accountId, httpClient);

                var failedCount = _failCounter.Get(accountId, httpClient);
                _nLogAdapter.Info(accountId, failedCount);
                _slackAdapter.Notify(accountId);
                    return false;
            }
        }
    }
}