using System;
using System.Net.Http;
#pragma warning disable 1591

namespace DependencyInjectionWorkshop.Models
{
    public interface IFailedCounter
    {
        void Reset(string accountId);
        void AddFailedCount(string accountId);

        /// <summary>
        /// Gets the account is locked.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        bool GetAccountIsLocked(string accountId);

        [AuditLog]
        int GetFailedCount(string accountId);
    }

    public class FailedCounter : IFailedCounter
    {
        public FailedCounter()
        {
        }

        public void Reset(string accountId)
        {
            HttpClient httpClient = new HttpClient() {BaseAddress = new Uri("http://joey.com/")};
            var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        public void AddFailedCount(string accountId)
        {
            HttpClient httpClient = new HttpClient() {BaseAddress = new Uri("http://joey.com/")};
            var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Gets the account is locked.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public bool GetAccountIsLocked(string accountId)
        {
            HttpClient httpClient = new HttpClient() {BaseAddress = new Uri("http://joey.com/")};
            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;

            isLockedResponse.EnsureSuccessStatusCode();
            var isLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            return isLocked;
        }

        public int GetFailedCount(string accountId)
        {
            var failedCountResponse =
                new HttpClient() {BaseAddress = new Uri("http://joey.com/")}.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }
    }
}