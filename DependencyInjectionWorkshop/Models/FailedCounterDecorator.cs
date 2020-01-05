namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounterDecorator : AuthenticationDecoratorBase
    {
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IAuthentication authentication, IFailedCounter failedCounter) : base(
            authentication)
        {
            _failedCounter = failedCounter;
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            CheckAccountLocked(accountId);
            var isValid = base.Verify(accountId, password, otp);
            if (isValid)
            {
                Reset(accountId);
            }
            else
            {
                AddFailedCount(accountId);
            }

            return isValid;
        }

        private void AddFailedCount(string accountId)
        {
            _failedCounter.AddFailedCount(accountId);
        }

        private void CheckAccountLocked(string accountId)
        {
            var isLocked = _failedCounter.GetAccountIsLocked(accountId);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() {AccountId = accountId};
            }
        }

        private void Reset(string accountId)
        {
            _failedCounter.Reset(accountId);
        }
    }
}