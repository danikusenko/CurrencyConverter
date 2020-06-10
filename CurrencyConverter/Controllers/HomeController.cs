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

namespace CurrencyConverter.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public List<Currency> GetCurrencies()
        {
            List<Currency> currencies = new List<Currency>();
            string xmlStr;
            System.Net.ServicePointManager.SecurityProtocol |=
            SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            using (var vc = new WebClient())
            {
                xmlStr = vc.DownloadString("https://www.nbrb.by/services/xmlexrates.aspx");
            }
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(new StringReader(xmlStr));
            XmlElement xRoot = xmlDoc.DocumentElement;
            foreach (XmlNode xnode in xRoot)
            {
                Currency currency = new Currency();
                foreach (XmlNode childNode in xnode.ChildNodes)
                {
                    if (childNode.Name == "Name")
                        currency.Name = childNode.InnerText;

                    if (childNode.Name == "Rate")
                        currency.Rate = double.Parse(childNode.InnerText, CultureInfo.InvariantCulture).ToString();

                    if (childNode.Name == "NumCode")
                        currency.NumCode = childNode.InnerText;

                    if (childNode.Name == "Scale")
                        currency.Scale = double.Parse(childNode.InnerText);
                }
                currencies.Add(currency);
            }
            currencies.Add(new Currency()
            {
                Name = "Белорусский рубль",
                Rate = "1",
                NumCode = "933",
                Scale = 1
            });
            return currencies;
        }

        public SelectList getAllNames()
        {
            List<string> names = new List<string>();
            foreach (var currency in GetCurrencies())
            {
                names.Add(currency.Name);
            }
            return new SelectList(names);
        }


        public void setDefaultValues(CurrencyViewModel currencyViewModel)
        {
            currencyViewModel.Value1 = currencyViewModel.Value1 ?? "1";
            foreach (var item in GetCurrencies())
            {
                if (item.NumCode == "933")
                    currencyViewModel.Name1 = currencyViewModel.Name1 ?? item.Name;

                if (item.NumCode == "840")
                {
                    currencyViewModel.Name2 = currencyViewModel.Name2 ?? item.Name;
                    currencyViewModel.Value2 = currencyViewModel.Value2 ?? item.Rate;
                }
            }
            currencyViewModel.Currencies = GetCurrencies();
            currencyViewModel.CurrencyNames = getAllNames();
        }

        public Currency GetCurrencyFromName(string name)
        {
            Currency currency = GetCurrencies().Where(i => i.Name == name).Select(i => i).First();
            return currency;
        }

        public void Calculate(CurrencyViewModel currencyViewModel)
        {
            double firstValue = double.Parse(currencyViewModel.Value1);
            double firstRate = double.Parse(GetCurrencyFromName(currencyViewModel.Name1).Rate) / 
                GetCurrencyFromName(currencyViewModel.Name1).Scale;
            double secondRate = double.Parse(GetCurrencyFromName(currencyViewModel.Name2).Rate) /
                GetCurrencyFromName(currencyViewModel.Name2).Scale;
            double secondValue = Math.Round(firstRate * firstValue / secondRate, 2);
            Console.WriteLine(firstValue);
            Console.WriteLine(secondValue);
            currencyViewModel.Value2 = secondValue.ToString();
        }
         
        public IActionResult Index(CurrencyViewModel currencyViewModel)
        {
            setDefaultValues(currencyViewModel);
            Calculate(currencyViewModel);
            Console.WriteLine(currencyViewModel.Value2);
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
