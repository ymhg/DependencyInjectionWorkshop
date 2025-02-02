﻿using NLog;

namespace DependencyInjectionWorkshop.Models
{
    public interface ILogger
    {
        void LogInfo(string message);
    }

    public class Logger : ILogger
    {
        public void LogInfo(string message)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }
}