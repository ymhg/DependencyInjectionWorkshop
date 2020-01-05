using System;
using System.Linq;
using Castle.DynamicProxy;
using DependencyInjectionWorkshop.Models;

namespace MyConsole
{
    public class AuditLogInterceptor : IInterceptor
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
            if (!(Attribute.GetCustomAttribute(invocation.Method, typeof(AuditLogAttribute)) is AuditLogAttribute
                auditLogAttribute))
            {
                invocation.Proceed();
            }
            else
            {
                var currentUser = _context.GetUser();
                var parameters = string.Join("|", invocation.Arguments.Select(x => (x ?? "").ToString()));

                var methodInfo = invocation.Method;

                var methodName = methodInfo.Name;
                _logger.Info(
                    $"[Audit method:{methodName}] user:{currentUser.Name} invoke with parameters:{parameters}");

                invocation.Proceed();

                var returnValue = invocation.ReturnValue;

                _logger.Info($"[Audit] return value:{returnValue}");
            }
        }
    }
}