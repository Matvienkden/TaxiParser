using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using TaxiParser.Core.Logger;
using TaxiParser.Core.Models;
using TaxiParser.Core.Extensions;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Threading;

namespace TaxiParser.Core
{
    public class ParserService
    {
        private ILogger logger { get; }
        private Uri basicUrl { get; }
        private int totalPage { get; set; }

        public ParserService(ILogger logger)
        {
            this.logger = logger;

            var targetUriString = ConfigurationManager.AppSettings.Get("TargetUrl");

            if (string.IsNullOrEmpty(targetUriString))
            {
                throw new ConfigurationErrorsException("В файле конфигурации не задан параметр TargetUrl (нужна ссылка на ресурс для парсинга)");
            }

            if (Uri.TryCreate(targetUriString, UriKind.Absolute, out var url) == false)
            {
                throw new ConfigurationErrorsException("В конфиге прописан некорретный url (параметр TargetUrl)");
            }

            basicUrl = new Uri(targetUriString);
        }

        public void Start()
        {

            while (true)
            {
                var results = new List<License>();
                try
                {
                    logger.Log("Запуск");
                    logger.Log("Определим кол-во страниц");

                    var web = new HtmlAgilityPack.HtmlWeb();                  
                    var doc = web.Load(basicUrl);
                    var totalRecordStr = new Regex(@"Документы\s\d+\s-\s\d+\sиз\s(\d+)")
                        .Match(doc.Text).Groups[1]
                        .ToString();

                    var totalRecord = int.Parse(totalRecordStr);
                    totalPage = (int)Math.Ceiling((double)totalRecord / 10);

                    logger.Log($"Всего записей: {totalRecord}");
                    logger.Log($"Всего страниц: {totalPage}");

                    logger.Log("\r\nЗапускаем парсинг страниц\r\n");

                    for (int i = 1; i <= totalPage; i++)
                    {
                        results.AddRange(GetAndParsingOnePage(i));
                        logger.Log($"Всего записей: {results.Count}");
                    }

                    logger.Log("\r\nВсе страницы успешно обработаны");

                    logger.Log("Сохраняем в файл");
                    Save(results);
                    logger.Log("Данные успешно сохранены");
                }
                catch (Exception e)
                {
                    logger.Log(e.Message);
                }

                logger.Log("\r\nСледующий запуск через 5 минут");
                Thread.Sleep(5*60*1000);
            }
        }

        private IEnumerable<License> GetAndParsingOnePage(int pageNumber)
        {
            logger.Log($"Грузим страницу {pageNumber} из {totalPage}");
            var parametrs = new Uri($"?PAGEN_2={pageNumber}#nav_start_2", UriKind.Relative);
            var request = WebRequest.Create(new Uri(basicUrl, parametrs));

            // ребята стабильно отдают страницу с кодом 404, так что будем его игнорировать
            Stream responseStream;
            try
            {
                responseStream = request.GetResponse().GetResponseStream();
            }
            catch (WebException ex)
            {
                responseStream = ex.Response.GetResponseStream();
                if (((HttpWebResponse)ex.Response).StatusCode != HttpStatusCode.NotFound || responseStream.Length < 20000)
                {
                    throw ex;
                }
            }

            logger.Log($"Страница {pageNumber} получена, начинам парсинг");

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.Load(responseStream, new UTF8Encoding());
            var table = doc.DocumentNode.SelectSingleNode("//table[@class=\"sticky-enabled\"]");
            var tempResults = table.SelectNodes("tr")
                .Skip(1)
                .Select(tr =>
                {
                    var tds = tr.SelectNodes("td").Select(td => td.InnerText).ToArray();
                    var fioArr = tds[2].DeleteExtraSpaces().Split(' ');
                    var isCompany = fioArr.Length > 3;
                    var endDate = DateTime.Parse(tds[6]);
                    return new License
                    {
                        LicenseNumber = tds[0],
                        CarNumber = tds[1],
                        LicenseOwner = isCompany ? tds[2].Replace("&quot;", "\"") : fioArr.FioFormat(),
                        IsCompany = isCompany,
                        CarVendor = tds[3],
                        CarModel = tds[4].ModelFormat(tds[3]),
                        LicenseStart = DateTime.Parse(tds[5]),
                        LicenseEnd = endDate,
                        State = string.IsNullOrEmpty(tds[7]) == false && endDate <= DateTime.Now ? tds[7] : "Действует"
                    };
                });

            logger.Log($"Успешно, получено записей со страницы: {tempResults.Count()}");
            return tempResults;
        }

        private void Save(IEnumerable<License> licenses)
        {
            using (var file = File.CreateText("licenses.json"))
            {
                var option = new JsonSerializerSettings();
                option.Formatting = Formatting.Indented;
                var js = JsonSerializer.Create(option);
                js.Serialize(file, licenses);
            }
        }
    }

    //if (basicUri.)
    //{

    //}

    //var request = HttpWebRequest.Create()

}

