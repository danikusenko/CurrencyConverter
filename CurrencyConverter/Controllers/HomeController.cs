using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CurrencyConverter.Models;
using Newtonsoft.Json.Linq;
using System.Net;
using CurrencyConverter.ViewModels;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using System.Text.Json;
using Newtonsoft.Json;
using System.ComponentModel;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Caching.Memory;
using CurrencyConverter.Services;

namespace CurrencyConverter.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private  List<Rate> rates; 
        private readonly IStringLocalizer _localizer;
        private  IMemoryCache _memoryCache;

        public HomeController(ILogger<HomeController> logger, IStringLocalizer localizer, IMemoryCache memoryCache)
        {
            _logger = logger;
            _localizer = localizer;
            _memoryCache = memoryCache;            
        }

        public async Task<Dictionary<string, decimal?>> loadChart(decimal? dividend_currency_rate, Rate rate,
            string gap = "month", bool byRub = false)
        {
            
            string courses, uri, format = "M", secondDate = "", firstDate = "";
            int[] primaryKeys = { 68, 184, 74, 77, 232, 27 };
            bool isMonthlyСourses = Array.Exists(primaryKeys, i => i == rate.Cur_ID || rate.Cur_ID >= 306);
            bool isYear = false;
            switch (gap)
            {
                case "five_days":
                    {
                        firstDate = DateTime.Today.AddDays(-4).ToString("yyyy-M-d");
                        secondDate = DateTime.Today.ToString("yyyy-M-d");
                        break;
                    }
                case "year":
                    {
                        firstDate = DateTime.Today.AddYears(-1).ToString("yyyy-M-d");
                        secondDate = DateTime.Today.ToString("yyyy-M-d");
                        format = "dd MMM yyyy";                        
                        isYear = true;
                        break;
                    }
                case "month":
                case null:
                    {
                        firstDate = DateTime.Today.AddMonths(-1).ToString("yyyy-M-d");
                        secondDate = DateTime.Today.ToString("yyyy-M-d");
                        break;
                    }
            }
            

            List<Rate> rates = new List<Rate>();
            if (!isMonthlyСourses)
            {
                uri = "https://www.nbrb.by/API/ExRates/Rates/Dynamics/" + rate.Cur_ID.ToString() +
                    "?startDate=" + firstDate + "&endDate=" + secondDate;
                using (var vc = new WebClient())
                {
                    courses = await vc.DownloadStringTaskAsync(new Uri(uri));
                }
                rates = JsonConvert.DeserializeObject<List<Rate>>(courses);
            }
            else
            {

                while (DateTime.Parse(firstDate) <= DateTime.Parse(secondDate))
                {
                    uri = "https://www.nbrb.by/api/exrates/rates/" + rate.Cur_ID + "?ondate=" + firstDate;
                    using (var vc = new WebClient())
                    {
                        courses = await vc.DownloadStringTaskAsync(new Uri(uri));
                    }
                    rates.Add(JsonConvert.DeserializeObject<Rate>(courses));
                    if (isYear)
                        firstDate = DateTime.Parse(firstDate).AddMonths(1).ToString("yyyy-M-d");
                    else
                        firstDate = DateTime.Parse(firstDate).AddDays(1).ToString("yyyy-M-d");
                }
            }


            Dictionary<string, decimal?> values = new Dictionary<string, decimal?>();
            if (byRub)
            {
                foreach (var _rate in rates)
                {
                    values.Add(_rate.Date.ToString(format), _rate.Cur_OfficialRate / rate.Cur_Scale);
                }
            }
            else
            {
                foreach (var _rate in rates)
                {
                    values.Add(_rate.Date.ToString(format), dividend_currency_rate / _rate.Cur_OfficialRate * rate.Cur_Scale);
                }
            }
            return values;
        }

        public SelectList getAllNames()
        {
            Dictionary<int, string> _rates = new Dictionary<int, string>();
            foreach (var rate in rates)
            {
                _rates.Add(rate.Cur_ID, _localizer[rate.Cur_Abbreviation]);
            }
            return new SelectList(_rates.OrderBy(x => x.Value), "Key", "Value");
        }


        public void setDefaultValues(CurrencyViewModel currencyViewModel)
        {
            currencyViewModel.Value1 = currencyViewModel.Value1 ?? 1;
            Rate usd = rates.Where(rate => rate.Cur_ID == 145).Select(i => i).First();
            currencyViewModel.Value2 = currencyViewModel.Value2 ?? usd.Cur_OfficialRate;
            currencyViewModel.Chart = currencyViewModel.Chart ?? loadChart(1, usd).Result;            
            currencyViewModel.Rates = currencyViewModel.Rates ?? rates;
            currencyViewModel.Cur_Id2 = currencyViewModel.Cur_Id2 ?? 145;
            currencyViewModel.Cur_Id1 = currencyViewModel.Cur_Id1 ?? 355;
            currencyViewModel.CurrencyNames = getAllNames();
        }


        public Rate GetRateFromId(int? id)
        {
            Rate rate = rates.Where(i => i.Cur_ID == id).Select(i => i).First();
            return rate;
        }


        public void Calculate(CurrencyViewModel currencyViewModel)
        {
            decimal? firstValue = currencyViewModel.Value1;
            decimal? firstRate = GetRateFromId(currencyViewModel.Cur_Id1).Cur_OfficialRate /
                GetRateFromId(currencyViewModel.Cur_Id1).Cur_Scale;
            decimal? secondRate = GetRateFromId(currencyViewModel.Cur_Id2).Cur_OfficialRate /
                GetRateFromId(currencyViewModel.Cur_Id2).Cur_Scale;
            decimal? secondValue = Math.Round((firstRate * firstValue.Value / secondRate).Value, 2);
            currencyViewModel.Value2 = secondValue;

        }

        public void CalculateFromSecondInput(CurrencyViewModel currencyViewModel)
        {
            decimal? secondValue = currencyViewModel.Value2;
            decimal? secondRate = GetRateFromId(currencyViewModel.Cur_Id2).Cur_OfficialRate /
                GetRateFromId(currencyViewModel.Cur_Id2).Cur_Scale;
            decimal? firstRate = GetRateFromId(currencyViewModel.Cur_Id1).Cur_OfficialRate /
                GetRateFromId(currencyViewModel.Cur_Id1).Cur_Scale;
            decimal? firstValue = Math.Round((secondRate * secondValue.Value / firstRate).Value, 2);
            currencyViewModel.Value1 = firstValue;

        }

        public void setValuesForChart(CurrencyViewModel currencyViewModel, string gap)
        {
            decimal? firstRate = GetRateFromId(currencyViewModel.Cur_Id1).Cur_OfficialRate /
                GetRateFromId(currencyViewModel.Cur_Id1).Cur_Scale;
            if (currencyViewModel.Cur_Id2 == 355)
            {
                currencyViewModel.Chart = loadChart(firstRate, GetRateFromId(currencyViewModel.Cur_Id1), gap, true).Result;                
            }
            else
            {
                currencyViewModel.Chart = loadChart(firstRate, GetRateFromId(currencyViewModel.Cur_Id2), gap).Result;               
            }
        }

        public IActionResult Index(CurrencyViewModel currencyViewModel, string from_two_input, string gap)
        {
            if (!_memoryCache.TryGetValue("rates", out rates))
            {
                rates = CurrencyService.GetRates().Result;//GetRates().Result;
            }
            setDefaultValues(currencyViewModel);
            if (from_two_input == "true")
                CalculateFromSecondInput(currencyViewModel);
            else
                Calculate(currencyViewModel);
            setValuesForChart(currencyViewModel, gap);
            ViewBag.Gap = gap;
            currencyViewModel.Name1 = rates.Where(m => m.Cur_ID == currencyViewModel.Cur_Id1).
                        Select(m => _localizer[m.Cur_Abbreviation]).FirstOrDefault();
            currencyViewModel.Name2 = rates.Where(m => m.Cur_ID == currencyViewModel.Cur_Id2).
                        Select(m => _localizer[m.Cur_Abbreviation]).FirstOrDefault();
            return View(currencyViewModel);
        }

        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
