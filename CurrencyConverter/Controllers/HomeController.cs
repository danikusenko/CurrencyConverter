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

namespace CurrencyConverter.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly List<Rate> rates = GetRates();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public static List<Rate> GetRates()
        {
            List<Currency> currencies = new List<Currency>();
            string dailyСourses, monthlyСourses;
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            using (var vc = new WebClient())
            {
                dailyСourses = vc.DownloadString("https://www.nbrb.by/api/exrates/rates?periodicity=0");
                monthlyСourses = vc.DownloadString("https://www.nbrb.by/api/exrates/rates?periodicity=1");
            }
            List<Rate> rates = JsonConvert.DeserializeObject<List<Rate>>(dailyСourses);
            rates.AddRange(JsonConvert.DeserializeObject<List<Rate>>(monthlyСourses));
            rates.Add(new Rate()
            {
                Cur_ID = 1,
                Cur_Name = "Белорусский рубль",
                Cur_Scale = 1,
                Cur_OfficialRate = 1
            });
            return rates;
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

        public IActionResult Index(CurrencyViewModel currencyViewModel, string from_two_input)
        {
            setDefaultValues(currencyViewModel);
            if (from_two_input == "true")
                CalculateFromSecondInput(currencyViewModel);
            else
                Calculate(currencyViewModel);
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
