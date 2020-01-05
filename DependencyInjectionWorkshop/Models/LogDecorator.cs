namespace DependencyInjectionWorkshop.Models
{
    public class LogDecorator : AuthenticationDecoratorBase
    {
        private readonly IFailedCounter _failedCounter;
        private readonly ILogger _logger;

        public LogDecorator(IAuthentication authentication, ILogger logger, IFailedCounter failedCounter) : base(
            authentication)
        {
            _logger = logger;
            _failedCounter = failedCounter;
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var isValid = base.Verify(accountId, password, otp);
            if (!isValid)
            {
                LogFailedCount(accountId);
            }

            return isValid;
        }

        private void LogFailedCount(string accountId)
        {
            //紀錄失敗次數 
            var failedCount = _failedCounter.GetFailedCount(accountId);
            _logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }
    }
}