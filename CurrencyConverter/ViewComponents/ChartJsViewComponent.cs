using CurrencyConverter.Models.Chart;
using CurrencyConverter.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyConverter.ViewComponents
{
    [ViewComponent(Name = "chartjs")]
    public class ChartJsViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(double[] data, string[] labels)
        {                   
            var chartData = @"
            {
                type: 'line',
                responsive: true,
                data:
                {
                    labels:" + new JArray(labels) + @",                    
                    datasets: [{
                        label: '# of Votes',
                        lineTension: 0,
                        data:" + new JArray(data) + @", 
                        backgroundColor: [
                        'rgba(2, 204, 39, 0.2)',                        
                            ],
                        borderColor: [
                        'rgba(0, 171, 31, 1)',
                        
                            ],
                        borderWidth: 2
                    }]
                },
                options:
                {      
                    scales:
                    {
                        yAxes: [{
                            ticks:
                            {
                                beginAtZero: false
                            }
                        }]
                    },
                    elements: {
                        point:{
                            radius: 0
                        }
                    },
                    legend: {
                        display: false
                    },
                    tooltips: {
                        enabled: false
                    }
                }
            }";
            var chart = JsonConvert.DeserializeObject<ChartJs>(chartData);

            var chartModel = new ChartJsViewModel
            {
                Chart = chart,
                ChartJson = JsonConvert.SerializeObject(chart, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                })
            };

            return View(chartModel);
        }
    }
}
