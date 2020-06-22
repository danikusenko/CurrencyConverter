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
using System.Xml;
using CurrencyConverter.ViewModels;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using System.Text.Json;
using Newtonsoft.Json;
using System.ComponentModel;

namespace CurrencyConverter.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly List<Rate> rates = GetRates().Result;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
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
                Cur_ID = 1,
                Cur_Name = "Белорусский рубль",
                Cur_Scale = 1,
                Cur_OfficialRate = 1,
                Date = DateTime.Today
            });
            return rates;
        }

        public async Task<Dictionary<string, double>> loadChart(double dividend_currency_rate, Rate rate,
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
                        format = "D";
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


            Dictionary<string, double> values = new Dictionary<string, double>();
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
            List<string> names = new List<string>();
            foreach (var rate in rates)
            {
                names.Add(rate.Cur_Name);
            }
            names.Sort();
            return new SelectList(names);
        }


        public void setDefaultValues(CurrencyViewModel currencyViewModel)
        {
            currencyViewModel.Value1 = currencyViewModel.Value1 ?? 1;
            foreach (var item in rates)
            {
                if (item.Cur_ID == 1)
                    currencyViewModel.Name1 = currencyViewModel.Name1 ?? item.Cur_Name;

                if (item.Cur_ID == 145)
                {
                    currencyViewModel.Name2 = currencyViewModel.Name2 ?? item.Cur_Name;
                    currencyViewModel.Value2 = currencyViewModel.Value2 ?? item.Cur_OfficialRate;
                }
            }
            Rate usd = rates.Where(rate => rate.Cur_ID == 145).Select(i => i).First();
            currencyViewModel.data = currencyViewModel.data ?? loadChart(1, usd).Result.Values.ToArray();
            currencyViewModel.labels = currencyViewModel.labels ?? loadChart(1, usd).Result.Keys.ToArray();
            currencyViewModel.Rates = rates;
            currencyViewModel.CurrencyNames = getAllNames();
        }

        public Rate GetRateFromName(string name)
        {
            Rate rate = rates.Where(i => i.Cur_Name == name).Select(i => i).First();
            return rate;
        }

        public void Calculate(CurrencyViewModel currencyViewModel)
        {
            double? firstValue = currencyViewModel.Value1;
            double firstRate = GetRateFromName(currencyViewModel.Name1).Cur_OfficialRate /
                GetRateFromName(currencyViewModel.Name1).Cur_Scale;
            double secondRate = GetRateFromName(currencyViewModel.Name2).Cur_OfficialRate /
                GetRateFromName(currencyViewModel.Name2).Cur_Scale;
            double secondValue = Math.Round(firstRate * firstValue.Value / secondRate, 2);
            currencyViewModel.Value2 = secondValue;

        }

        public void CalculateFromSecondInput(CurrencyViewModel currencyViewModel)
        {
            double? secondValue = currencyViewModel.Value2;
            double secondRate = GetRateFromName(currencyViewModel.Name2).Cur_OfficialRate /
                GetRateFromName(currencyViewModel.Name2).Cur_Scale;
            double firstRate = GetRateFromName(currencyViewModel.Name1).Cur_OfficialRate /
                GetRateFromName(currencyViewModel.Name1).Cur_Scale;
            double firstValue = Math.Round(secondRate * secondValue.Value / firstRate, 2);
            currencyViewModel.Value1 = firstValue;

        }

        public void setValuesForChart(CurrencyViewModel currencyViewModel, string gap)
        {
            double firstRate = GetRateFromName(currencyViewModel.Name1).Cur_OfficialRate /
                GetRateFromName(currencyViewModel.Name1).Cur_Scale;
            if (GetRateFromName(currencyViewModel.Name2).Cur_ID == 1)
            {
                currencyViewModel.labels = loadChart(firstRate, GetRateFromName(currencyViewModel.Name1), gap, true).Result.Keys.ToArray();
                currencyViewModel.data = loadChart(firstRate, GetRateFromName(currencyViewModel.Name1), gap, true).Result.Values.ToArray();
            }
            else
            {
                currencyViewModel.labels = loadChart(firstRate, GetRateFromName(currencyViewModel.Name2), gap).Result.Keys.ToArray();
                currencyViewModel.data = loadChart(firstRate, GetRateFromName(currencyViewModel.Name2), gap).Result.Values.ToArray();
            }
        }

        public IActionResult Index(CurrencyViewModel currencyViewModel, string from_two_input, string gap)
        {
            setDefaultValues(currencyViewModel);
            if (from_two_input == "true")
                CalculateFromSecondInput(currencyViewModel);
            else
                Calculate(currencyViewModel);
            setValuesForChart(currencyViewModel, gap);
            ViewBag.Gap = gap;
            return View(currencyViewModel);
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
