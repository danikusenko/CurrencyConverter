using CurrencyConverter.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace CurrencyConverter.Services
{
    public class CurrencyService : BackgroundService
    {
        private readonly IMemoryCache memoryCache;

        public CurrencyService(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
        }

        public async static Task<List<Rate>> GetRates()
        {
            string dailyСourses, monthlyСourses;
            using (var vc = new WebClient())
            {
                dailyСourses = await vc.DownloadStringTaskAsync(new Uri("https://www.nbrb.by/api/exrates/rates?periodicity=0"));
                monthlyСourses = await vc.DownloadStringTaskAsync(new Uri("https://www.nbrb.by/api/exrates/rates?periodicity=1"));
            }
            List<Rate> rates = JsonConvert.DeserializeObject<List<Rate>>(dailyСourses);
            rates.AddRange(JsonConvert.DeserializeObject<List<Rate>>(monthlyСourses));
            rates.Add(new Rate()
            {
                Cur_ID = 355,
                Cur_Name = "Белорусский рубль",
                Cur_Scale = 1,
                Cur_OfficialRate = 1,
                Date = DateTime.Today,
                Cur_Abbreviation = "BY"
            });
            return rates;
        } 

        public async static Task<List<Currency>> GetCurrencies()
        {
            string cur;
            using (var vc = new WebClient())
            {
                cur = await vc.DownloadStringTaskAsync(new Uri("https://www.nbrb.by/api/exrates/currencies"));
            }
            List<Currency> currencies = JsonConvert.DeserializeObject<List<Currency>>(cur);
            currencies.Add(new Currency()
            {
                Cur_ID = 355,
                Cur_Name = "Белорусский рубль",
                Cur_Scale = 1,
                Cur_Name_Bel = "Беларускі рубель",
                Cur_Name_Eng = "Belarusian ruble",
                Cur_Abbreviation = "BY"
            });
            return currencies;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string dailyСourses = "", monthlyСourses = "", cur = "";            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var vc = new WebClient())
                    {
                        dailyСourses = await vc.DownloadStringTaskAsync(new Uri("https://www.nbrb.by/api/exrates/rates?periodicity=0"));
                        monthlyСourses = await vc.DownloadStringTaskAsync(new Uri("https://www.nbrb.by/api/exrates/rates?periodicity=1"));
                        cur = await vc.DownloadStringTaskAsync(new Uri("https://www.nbrb.by/api/exrates/currencies"));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                List<Rate> rates = JsonConvert.DeserializeObject<List<Rate>>(dailyСourses);
                rates.AddRange(JsonConvert.DeserializeObject<List<Rate>>(monthlyСourses));
                rates.Add(new Rate()
                {
                    Cur_ID = 355,
                    Cur_Name = "Белорусский рубль",
                    Cur_Scale = 1,
                    Cur_OfficialRate = 1,
                    Date = DateTime.Today,
                    Cur_Abbreviation = "BY"
                });
                List<Currency> currencies = JsonConvert.DeserializeObject<List<Currency>>(cur);
                currencies.Add(new Currency()
                {
                    Cur_ID = 355,
                    Cur_Name = "Белорусский рубль",
                    Cur_Scale = 1,
                    Cur_Name_Bel = "Беларускі рубель",
                    Cur_Name_Eng = "Belarusian ruble",
                    Cur_Abbreviation = "BY"
                });                
                memoryCache.Set("rates", GetRates().Result, TimeSpan.FromMinutes(1440));
                memoryCache.Set("currencies", GetCurrencies().Result, TimeSpan.FromMinutes(1440));
                await Task.Delay(3600000, stoppingToken);
            }
        }
    }
}
