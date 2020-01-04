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

        public AuthenticationService()
        {
            _profileDao = new ProfileDao();
            _sha256Adapter = new Sha256Adapter();
            _slackAdapter = new SlackAdapter();
            _failCounter = new FailCounter();
            _otpAdapter = new OtpAdapter();
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
            
            //check account locked
            var isLocked = _failCounter.IsLocked(accountId, httpClient);
            if (isLocked)
            {
                throw new FailedTooManyTimesException(){AccountId = accountId};
            }
            //// get password
            var passwordFromDb = _profileDao.PasswordFromDb(accountId);

            //// get hash
            var hashedPassword = _sha256Adapter.HashedPassword(password);

            //// get otp
            var currentOtp = _otpAdapter.GetOtp(accountId, httpClient);

            //// compare
            if (currentOtp == otp && hashedPassword == passwordFromDb)
            {
                // 失敗次數歸0
                _failCounter.Reset(accountId, httpClient);
                return true;
            }
            else
            {
                _failCounter.Add(accountId, httpClient);

                LogFailedCount(accountId, httpClient);
                _slackAdapter.Notify(accountId);
                return false;
            }
        }

        private void LogFailedCount(string accountId, HttpClient httpClient)
        {
            // failed log 
            var failedCountResponse =
                httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}