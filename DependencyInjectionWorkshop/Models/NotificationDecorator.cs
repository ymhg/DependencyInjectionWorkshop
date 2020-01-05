namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator : AuthenticationDecoratorBase
    {
        private readonly INotification _notification;
        private readonly ILogger _logger;

        public NotificationDecorator(IAuthentication authentication, INotification notification, ILogger logger) : base(authentication)
        {
            _notification = notification;
            _logger = logger;
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var isValid = base.Verify(accountId, password, otp);
            if (!isValid)
            {
                _logger.Info("hello world");
                Notify(accountId);
            }

            return isValid;
        }

        private void Notify(string accountId)
        {
            _notification.Notify(accountId, $"account:{accountId} try to login failed");
        }
    }
}