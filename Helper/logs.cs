using SN_MarketUygulamasi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SN_MarketUygulamasi.Logs
{
    public class Logger
    {
        private static Logger _instance;
        private static readonly object _lock = new object();
        private readonly DBRR_AIEntities2 _dbContext;

        private Logger()
        {
            _dbContext = new DBRR_AIEntities2();
        }

        public static Logger GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new Logger();
                    }
                }
            }
            return _instance;
        }

        public void Log(string tabloAdi, string logType, string logDesc, string oldValues = null, string newValues = null)
        {
            try
            {
                var logEntry = new SN_M_logs
                {
                    tabloAdı = tabloAdi,
                    logType = logType,
                    logDesc = logDesc,
                    logTime = DateTime.Now,
                    oldValues = oldValues,
                    newValues = newValues
                };

                _dbContext.SN_M_logs.Add(logEntry);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Log kaydedilirken hata oluştu: {ex.Message}");
            }
        }
    }
}