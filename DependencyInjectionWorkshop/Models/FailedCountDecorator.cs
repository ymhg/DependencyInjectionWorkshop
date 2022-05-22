namespace DependencyInjectionWorkshop.Models
{
    public class FailedCountDecorator : IAuthentication
    {
        private readonly IAuthentication _authentication;
        private readonly IFailedCounter _failedCounter;

        public FailedCountDecorator(IAuthentication authentication, IFailedCounter failedCounter)
        {
            _authentication = authentication;
            _failedCounter = failedCounter;
        }

        private void CheckAccountLocked(string accountId)
        {
            var isAccountLocked = _failedCounter.IsAccountLocked(accountId);
            if (isAccountLocked)
            {
                throw new FailedTooManyTimesException { AccountId = accountId };
            }
        }

        public bool Verify(string accountId, string inputPassword, string inputOtp)
        {
            CheckAccountLocked(accountId);
            return _authentication.Verify(accountId, inputPassword, inputOtp);
        }
    }
}