using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TelegramBot.Models;

namespace TestConsole.Services
{
    public class PrivatBank
    {
        const string data_url = @"https://api.privatbank.ua/p24api/exchange_rates?json&date=";
        readonly string _Currency;
        readonly string _OnDate;
        readonly ApiResponseModel _Responce;
        readonly HttpClient _Client;

        public PrivatBank(string OnDate, string Currency)
        {
            _OnDate = OnDate;
            _Currency = Currency;
            _Client = new HttpClient();
            _Responce = GetData();
        }

        private async Task<Stream> GetDataStream()
        {
            var response = await _Client.GetAsync(
                data_url + _OnDate,
                HttpCompletionOption.ResponseHeadersRead);
            return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }
        private IEnumerable<string> GetDataLines()
        {
            using var data_stream = (SynchronizationContext.Current is null ? GetDataStream() : Task.Run(GetDataStream)).Result;
            using var data_reader = new StreamReader(data_stream);

            while (!data_reader.EndOfStream)
            {
                var line = data_reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                yield return line;
            }
        }
        private ApiResponseModel GetData()
        {
            try
            {
                return JsonConvert.DeserializeObject<ApiResponseModel>(GetDataLines().First(), new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
            }
            catch (Exception)
            {

                return new ApiResponseModel();
            }
        }
        private string GetMyCurrency()
        {
            return String.Join(", ", (_Responce.ExchangeRate.Select(s => s.Currency).ToArray()));
        }

        public bool IHaveExchangeRate()
        {
            return _Responce.ExchangeRate.Count > 0;
        }

        public string GetExchangeRate()
        {
            if (IHaveExchangeRate())
            {
                var temp = _Responce.ExchangeRate.Select(s => s).Where(c => c.Currency == _Currency);
                if (temp.Any())
                    return temp.First().ToString();
            }
            return "I don't have " + _Currency + " currency. But there is " + GetMyCurrency();
        }
    }
}
