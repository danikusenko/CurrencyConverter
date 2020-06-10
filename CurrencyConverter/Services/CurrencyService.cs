using CurrencyConverter.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CurrencyConverter.Services
{
    public class CurrencyService: BackgroundService
    {
        public class RootObject
        {
            public string code { get; set; }
            public string number { get; set; }
            public string name { get; set; }
        }

        private readonly IMemoryCache memoryCache;

        public CurrencyService(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                List<Currency> currencies = new List<Currency>();
                try
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    XDocument xml = XDocument.Load("https://www.nbrb.by/services/xmlexrates.aspx");
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            await Task.Delay(3600000, stoppingToken);
        }
    }
}
