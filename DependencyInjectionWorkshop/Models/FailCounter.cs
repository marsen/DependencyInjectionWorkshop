using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public interface IFailedCounter
    {
        /// <summary>
        /// Determines whether the specified account identifier is locked.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        bool IsLocked(string accountId);

        void Reset(string accountId);
        void Add(string accountId);
        [AuditLog]
        int Get(string accountId);
    }

    public class FailedCounter : IFailedCounter
    {
        public FailedCounter()
        {
        }

        /// <summary>
        /// Determines whether the specified account identifier is locked.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        public bool IsLocked(string accountId)
        {
            var isLockedResponse = new HttpClient() {BaseAddress = new Uri("http://joey.com/")}
                .PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;
            isLockedResponse.EnsureSuccessStatusCode();
            var IsLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            return IsLocked;
        }

        public void Reset(string accountId)
        {
            var resetResponse = new HttpClient() {BaseAddress = new Uri("http://joey.com/")}
                .PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        public void Add(string accountId)
        {
            //失敗
            var addFailedCountResponse = new HttpClient() {BaseAddress = new Uri("http://joey.com/")}
                .PostAsJsonAsync("api/failedCounter/Add", accountId).Result;

            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        public int Get(string accountId)
        {
            // failed log 
            var failedCountResponse =
                new HttpClient() {BaseAddress = new Uri("http://joey.com/")}
                    .PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }
    }

    public class AuditLogAttribute : Attribute
    {
    }
}