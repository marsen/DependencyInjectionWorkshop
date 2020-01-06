using System;
using System.Net.Http;
using DependencyInjectionWorkshop.Models.Interface;

namespace DependencyInjectionWorkshop.Models
{
    public class OtpService : IOtpService
    {
        public string CurrentOtp(string accountId, HttpClient httpClient)
        {
            //get otp
            var response = httpClient.PostAsJsonAsync("api/otps", accountId).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"web api error, accountId:{accountId}");
            }

            var currentOtp = response.Content.ReadAsAsync<string>().Result;
            return currentOtp;
        }
    }
}