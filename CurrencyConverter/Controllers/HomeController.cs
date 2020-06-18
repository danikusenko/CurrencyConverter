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

        public async Task<Dictionary<string, double>> loadChart(double dividend_currency_rate, int currency_id,
            int scale, string gap = "month")
        {
            string courses, firstDate = "";
            if (gap == "five_days")
                firstDate = DateTime.Today.AddDays(-4).ToString("yyyy-M-d");
            else if (gap == "year")
                firstDate = DateTime.Today.AddYears(-1).ToString("yyyy-M-d");
            else if (gap == "month" || gap == null)
            {
                firstDate = DateTime.Today.AddMonths(-1).ToString("yyyy-M-d");
            }
            string secondDate = DateTime.Today.ToString("yyyy-M-d");
            string uri = "https://www.nbrb.by/API/ExRates/Rates/Dynamics/" + currency_id.ToString() +
                "?startDate=" + firstDate + "&endDate=" + secondDate;
            using (var vc = new WebClient())
            {
                courses = await vc.DownloadStringTaskAsync(new Uri(uri));
            }
            List<Rate> rates = JsonConvert.DeserializeObject<List<Rate>>(courses);
            Dictionary<string, double> values = new Dictionary<string, double>();
            foreach (var rate in rates)
            {
                values.Add(rate.Date.ToString("M"), dividend_currency_rate / rate.Cur_OfficialRate / scale);
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
            currencyViewModel.data = currencyViewModel.data ?? loadChart(1, 145, 1).Result.Values.ToArray();
            currencyViewModel.labels = currencyViewModel.labels ?? loadChart(1, 145, 1).Result.Keys.ToArray();
            currencyViewModel.Rates = rates;
            currencyViewModel.CurrencyNames = getAllNames();
        }

        public Rate GetRateFromName(string name)
        {
            Rate rate = rates.Where(i => i.Cur_Name == name).Select(i => i).First();
            return rate;
        }

        public void Calculate(CurrencyViewModel currencyViewModel, string gap)
        {
            double? firstValue = currencyViewModel.Value1;
            double firstRate = GetRateFromName(currencyViewModel.Name1).Cur_OfficialRate /
                GetRateFromName(currencyViewModel.Name1).Cur_Scale;
            double secondRate = GetRateFromName(currencyViewModel.Name2).Cur_OfficialRate /
                GetRateFromName(currencyViewModel.Name2).Cur_Scale;
            double secondValue = Math.Round(firstRate * firstValue.Value / secondRate, 2);
            currencyViewModel.Value2 = secondValue;
            int secondCurrencyId = GetRateFromName(currencyViewModel.Name2).Cur_ID;
            int scale = GetRateFromName(currencyViewModel.Name2).Cur_Scale;
            currencyViewModel.labels = loadChart(firstRate, secondCurrencyId, scale, gap).Result.Keys.ToArray();
            currencyViewModel.data = loadChart(firstRate, secondCurrencyId, scale, gap).Result.Values.ToArray();
           
        }

        public void CalculateFromSecondInput(CurrencyViewModel currencyViewModel, string gap)
        {
            double? secondValue = currencyViewModel.Value2;
            double secondRate = GetRateFromName(currencyViewModel.Name2).Cur_OfficialRate /
                GetRateFromName(currencyViewModel.Name2).Cur_Scale;
            double firstRate = GetRateFromName(currencyViewModel.Name1).Cur_OfficialRate /
                GetRateFromName(currencyViewModel.Name1).Cur_Scale;
            double firstValue = Math.Round(secondRate * secondValue.Value / firstRate, 2);
            currencyViewModel.Value1 = firstValue;
            /*int firstCurrencyId = GetRateFromName(currencyViewModel.Name1).Cur_ID;
            int scale = GetRateFromName(currencyViewModel.Name1).Cur_Scale;
            currencyViewModel.labels = loadChart(secondRate, firstCurrencyId, scale).Result.Keys.ToArray();            
            currencyViewModel.data = loadChart(secondRate, firstCurrencyId, scale).Result.Values.ToArray();*/
        }

        public IActionResult Index(CurrencyViewModel currencyViewModel, string from_two_input, string gap)
        {            
            setDefaultValues(currencyViewModel);
            if (from_two_input == "true")
                CalculateFromSecondInput(currencyViewModel, gap);
            else
                Calculate(currencyViewModel, gap);
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
