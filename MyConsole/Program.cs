using System;
using Autofac;
using Autofac.Extras.DynamicProxy;
using Castle.DynamicProxy;
using DependencyInjectionWorkshop.Models;
using DependencyInjectionWorkshop.Models.Decorator;
using DependencyInjectionWorkshop.Models.Interface;

namespace MyConsole
{
    internal class Program
    {
        private static IContainer _container;

        private static void Main(string[] args)
        {
            RegisterContainer();
            Login("Marsen's agent");
            var authentication = _container.Resolve<IAuthentication>();
            var isValid = authentication.Verify("joey", "abc", "wrong otp");
            Console.WriteLine($"result:{isValid}");
            Console.ReadLine();
        }

        private static void Login(string agent)
        {
            IContext context = _container.Resolve<IContext>();
            context.SetUser(agent);
        }

        private static void RegisterContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<FakeOtp>().As<IOtpService>();
            builder.RegisterType<FakeHash>().As<IHash>();
            builder.RegisterType<FakeProfile>().As<IProfile>();
            builder.RegisterType<FakeLogger>().As<ILogger>();
            builder.RegisterType<FakeSlack>().As<INotification>();
            builder.RegisterType<FakeFailedCounter>().As<IFailedCounter>();

            builder.RegisterType<FakeContext>().As<IContext>().SingleInstance();
            builder.RegisterType<AuditLogInterceptor>();

            builder.RegisterType<AuthenticationService>().As<IAuthentication>()
                .EnableInterfaceInterceptors()
                .InterceptedBy(typeof(AuditLogInterceptor)); 

            builder.RegisterDecorator<FailedCounterDecorator, IAuthentication>();
            builder.RegisterDecorator<LoggerDecorator, IAuthentication>();
            builder.RegisterDecorator<NotificationDecorator, IAuthentication>();
            //builder.RegisterDecorator<LogMethodInfoDecorator, IAuthentication>();
            builder.RegisterDecorator<AuditLogDecorator, IAuthentication>();

            _container = builder.Build();
        }

        internal class FakeLogger : ILogger
        {
            public void Info(string message)
            {
                Console.WriteLine($"Logger: {message}");
            }
        }

        internal class FakeSlack : INotification
        {
            public void PushMessage(string message)
            {
                Console.WriteLine(message);
            }

            public void Notify(string accountId, string message)
            {
                PushMessage($"{nameof(Notify)}, accountId:{accountId}, message:{message}");
            }
        }

        internal class FakeFailedCounter : IFailedCounter
        {
            public void Reset(string accountId)
            {
                Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Reset)}({accountId})");
            }

            public void Add(string accountId)

            {
                Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Add)}({accountId})");
            }

            public int Get(string accountId)
            {
                Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Get)}({accountId})");
                return 91;
            }

            public bool IsLocked(string accountId)
            {
                Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(IsLocked)}({accountId})");
                return false;
            }
        }

        internal class FakeOtp : IOtpService
        {
            public string GetOtp(string accountId)
            {
                Console.WriteLine($"{nameof(FakeOtp)}.{nameof(GetOtp)}({accountId})");
                return "123456";
            }
        }

        internal class FakeHash : IHash
        {
            public string ComputeHash(string plainText)
            {
                Console.WriteLine($"{nameof(FakeHash)}.{nameof(ComputeHash)}({plainText})");
                return "my hashed password";
            }
        }

        internal class FakeProfile : IProfile
        {
            public string Password(string accountId)
            {
                Console.WriteLine($"{nameof(FakeProfile)}.{nameof(Password)}({accountId})");
                return "my hashed password";
            }
        }
    }

    internal class AuditLogInterceptor:IInterceptor
    {
        private readonly ILogger _logger;
        private readonly IContext _context;

        public AuditLogInterceptor(ILogger logger, IContext context)
        {
            _logger = logger;
            _context = context;
        }

        public void Intercept(IInvocation invocation)
        {
            _logger.Info($"[Audit - {invocation.Method.Name} input]user {_context.GetUser().Name},parameters:{invocation.Arguments[0]}| {invocation.Arguments[1]}|{invocation.Arguments[2]}");
            invocation.Proceed();
            _logger.Info($"[Audit - {invocation.Method.Name} output]verify is {invocation.ReturnValue}");

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
            _user = new User{Name = userName};
        }
    }

    internal class AuditLogDecorator:AuthenticationDecoratorBase
    {
        private readonly ILogger _logger;
        private readonly IContext _context;

        public AuditLogDecorator(IAuthentication authenticationService, ILogger logger, IContext context) : base(authenticationService)
        {
            _logger = logger;
            this._context = context;
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            _logger.Info($"user {_context.GetUser().Name},parameters:{accountId}| {password}|{otp}");
            var verify = base.Verify(accountId, password, otp);
            _logger.Info($"verify is {verify}");
            return verify;
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

    internal class LogMethodInfoDecorator : AuthenticationDecoratorBase
    {
        private readonly ILogger _logger;

        public LogMethodInfoDecorator(IAuthentication authenticationService, ILogger logger) : base(
            authenticationService)
        {
            _logger = logger;
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            _logger.Info($"parameters:{accountId}| {password}|{otp}");
            var verify = base.Verify(accountId, password, otp);
            _logger.Info($"verify is {verify}");
            return verify;
        }
    }
}