using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class FailCounter
    {
        public FailCounter()
        {
        }

        public bool IsLocked(string accountId, HttpClient httpClient)
        {
            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;
            isLockedResponse.EnsureSuccessStatusCode();
            var IsLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            return IsLocked;
        }

        public void Reset(string accountId, HttpClient httpClient)
        {
            var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        public void Add(string accountId, HttpClient httpClient)
        {
            //失敗
            var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;

            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        public int Get(string accountId, HttpClient httpClient)
        {
            // failed log 
            var failedCountResponse =
                httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }
    }
}