using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class OtpService
    {
        public OtpService()
        {
        }

        /// <summary>
        /// Gets the current otp.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="httpClient">The HTTP client.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">web api error, accountId:{accountId}</exception>
        public string GetCurrentOtp(string accountId, HttpClient httpClient)
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