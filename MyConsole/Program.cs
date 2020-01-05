using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using DependencyInjectionWorkshop.Models;

namespace MyConsole
{
    class Program
    {
        private static IContainer _container;
        //private static IContext _context;

        static void Main(string[] args)
        {
            RegisterContainer();

            Login("joey's agent");

            var authentication = _container.Resolve<IAuthentication>();

            var isValid = authentication.Verify("joey", "abc", "wrong otp");
            Console.WriteLine($"result:{isValid}");
        }

        private static void Login(string userName)
        {
            IContext context = _container.Resolve<IContext>();
            context.SetUser(userName);
        }

        private static void RegisterContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<FakeProfile>().As<IProfile>();
            builder.RegisterType<FakeOtp>().As<IOtpService>();
            builder.RegisterType<FakeHash>().As<IHash>();
            builder.RegisterType<FakeLogger>().As<ILogger>();
            builder.RegisterType<FakeFailedCounter>().As<IFailedCounter>();
            builder.RegisterType<FakeSlack>().As<INotification>();

            builder.RegisterType<FakeContext>().As<IContext>().SingleInstance();

            builder.RegisterType<AuthenticationService>().As<IAuthentication>();

            builder.RegisterDecorator<FailedCounterDecorator, IAuthentication>();
            builder.RegisterDecorator<LogDecorator, IAuthentication>();
            builder.RegisterDecorator<NotificationDecorator, IAuthentication>();
            //builder.RegisterDecorator<LogMethodInfoDecorator, IAuthentication>();
            builder.RegisterDecorator<AuditLogDecorator, IAuthentication>();

            _container = builder.Build();
        }
    }

    internal class FakeContext : IContext
    {
        private User _user;

        public User GetUser()
        {
            return _user;
        }

        public void SetUser(string userName)
        {
            _user = new User(){Name = userName};
        }
    }

    internal class AuditLogDecorator : AuthenticationDecoratorBase
    {
        private readonly ILogger _logger;
        private readonly IContext _context;

        public AuditLogDecorator(IAuthentication authentication, ILogger logger, IContext context) : base(
            authentication)
        {
            _logger = logger;
            _context = context;
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            string userName = _context.GetUser().Name;
            _logger.Info($"user:{userName}, parameters:{accountId} | {password} | {otp}");
            var isValid = base.Verify(accountId, password, otp);
            _logger.Info($"return value:{isValid}");
            return isValid;
        }
    }

    public interface IContext
    {
        User GetUser();
        void SetUser(string userName);
    }

    public class User
    {
        public string Name { get; set; }
    }

    internal class FakeLogger : ILogger
    {
        public void Info(string message)
        {
            Console.WriteLine($"logger: {message}");
        }
    }

    internal class FakeSlack : INotification
    {
        public void Notify(string accountId, string message)
        {
            PushMessage($"{nameof(Notify)}, accountId:{accountId}, message:{message}");
        }

        public void PushMessage(string message)
        {
            Console.WriteLine(message);
        }
    }

    internal class FakeFailedCounter : IFailedCounter
    {
        public void Reset(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Reset)}({accountId})");
        }

        public void AddFailedCount(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(AddFailedCount)}({accountId})");
        }

        public bool GetAccountIsLocked(string accountId)
        {
            return IsAccountLocked(accountId);
        }

        public int GetFailedCount(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(GetFailedCount)}({accountId})");
            return 91;
        }

        public bool IsAccountLocked(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(IsAccountLocked)}({accountId})");
            return false;
        }
    }

    internal class FakeOtp : IOtpService
    {
        public string GetCurrentOtp(string accountId)
        {
            Console.WriteLine($"{nameof(FakeOtp)}.{nameof(GetCurrentOtp)}({accountId})");
            return "123456";
        }
    }

    internal class FakeHash : IHash
    {
        public string Compute(string plainText)
        {
            Console.WriteLine($"{nameof(FakeHash)}.{nameof(Compute)}({plainText})");
            return "my hashed password";
        }
    }

    internal class FakeProfile : IProfile
    {
        public string GetPassword(string accountId)
        {
            Console.WriteLine($"{nameof(FakeProfile)}.{nameof(GetPassword)}({accountId})");
            return "my hashed password";
        }
    }
}