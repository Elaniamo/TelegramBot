using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TelegramBot.Models
{
    public class ApiResponseModel
    {
        [JsonProperty(PropertyName = "Date")]
        DateTime Date { get; set; }
        [JsonProperty(PropertyName = "Bank")]
        string Bank { get; set; }
        [JsonProperty(PropertyName = "BaseCurrency")]
        int BaseCurrency { get; set; }
        [JsonProperty(PropertyName = "BaseCurrencyLit")]
        string BaseCurrencyLit { get; set; }

        [JsonProperty(PropertyName = "ExchangeRate")]
        public List<ExchangeRateItems> ExchangeRate { get; private set; }
    }

    public class ExchangeRateItems
    {
        [JsonProperty(PropertyName = "BaseCurrency")]
        public string BaseCurrency { get; private set; }

        [JsonProperty(PropertyName = "Currency")]
        public string Currency { get; private set; }

        [JsonProperty(PropertyName = "SaleRateNB")]
        public double SaleRateNB { get; private set; }

        [JsonProperty(PropertyName = "SaleRate")]
        public double SaleRate { get; private set; }

        [JsonProperty(PropertyName = "PurchaseRate")]
        public double PurchaseRate { get; private set; }

        public override string ToString()
        {
            return $"Base currency: {BaseCurrency} \nCurrency: {Currency}\nSale rate National Bank: {string.Format("{0:0.00}", SaleRateNB)}\n" +
                $"Sale rate: {string.Format("{0:0.00}", SaleRate)}\nPurchase rate: {string.Format("{0:0.00}", PurchaseRate)}";
        }
    }
}
