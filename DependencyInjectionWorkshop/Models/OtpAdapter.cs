using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class OtpAdapter
    {
        public OtpAdapter()
        {
        }

        public string GetOtp(string accountId)
        {
            var response = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/otps", accountId).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"web api error, accountId:{accountId}");
            }

            var currentOtp = response.Content.ReadAsAsync<string>().Result;
            return currentOtp;
        }
    }
}