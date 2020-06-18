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
            JArray colors = new JArray();
            JArray borderColor = new JArray();
            if (data.Length > 0)
            {
                if (data[0] > data[data.Length - 1])
                {
                    colors.Add("rgba(230, 74, 25, 0.2)");
                    borderColor.Add("rgba(230, 74, 25, 1)");
                }
                else
                {
                    colors.Add("rgba(15, 157, 88, 0.2)");
                    borderColor.Add("rgba(15, 157, 88, 1)");
                }
            }
            var chartData = @"
            {
                type: 'line',
                responsive: true,
                data:
                {
                    labels:" + new JArray(labels) + @",                    
                    datasets: [{
                        label: '',
                        lineTension: 0,
                        data:" + new JArray(data) + @", 
                        backgroundColor:" + colors + @",
                        borderColor:" + borderColor + @",
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
                            },
                            scaleLabel:
                            {
                                display: true
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
                        enabled: true
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
