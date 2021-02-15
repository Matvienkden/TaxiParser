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
using HtmlAgilityPack;
using TaxiParser.Core.Enums;
using TaxiParser.Core.Models.Licenses;

namespace TaxiParser.Core.Parsers
{
    /// <summary>
    /// Парсер такси для Волгоградской области
    /// </summary>
    public class VolgogradParser : ParserBase<VolgogradLicense>
    {
        private int totalPage { get; set; }
        public VolgogradParser(ILogger logger, SaveTo saveTo) : base(logger, saveTo) { }

        protected override void GoParsing()
        {
            logger.Log("\nОпределим кол-во страниц");

            var web = new HtmlWeb();
            var doc = web.Load(basicUrl);

            var totalRecordStr = doc.Text.GetStrByRegexGroup(@"Документы\s\d+\s-\s\d+\sиз\s(\d+)", 1);

            var totalRecord = int.Parse(totalRecordStr);
            totalPage = (int)Math.Ceiling((double)totalRecord / 10);

            logger.Log($"Всего записей: {totalRecord}");
            logger.Log($"Всего страниц: {totalPage}", LogLevel.SUCCSESS);

            logger.Log("\r\nЗапускаем парсинг страниц\r\n");

            var resultCount = 0;
            for (int i = 1; i <= totalPage; i++)
            {
                try
                {
                    var results = GetAndParsingOnePage(i);                
                    resultCount += results.Count();
                    Save(results);

                    logger.Log($"Всего получено записей записей: {resultCount}");
                }
                catch (Exception e)
                {
                    logger.Log(e.Message, LogLevel.ERROR);
                }
            }

            logger.Log("\r\nВсе страницы успешно обработаны", LogLevel.SUCCSESS);
        }

        private IEnumerable<VolgogradLicense> GetAndParsingOnePage(int pageNumber)
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

            logger.Log($"Страница {pageNumber} получена, начинам парсинг", LogLevel.SUCCSESS);

            var doc = new HtmlDocument();
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
                    return new VolgogradLicense
                    {
                        LicenseNumber = tds[0],
                        CarNumber = tds[1].RegexReplace(@"\s+", ""),
                        LicenseOwner = isCompany ? tds[2].Replace("&quot;", "\"") : fioArr.FioFormat(),
                        IsCompany = isCompany,
                        CarVendor = tds[3],
                        CarModel = tds[4].ModelFormat(tds[3]),
                        LicenseStart = DateTime.Parse(tds[5]),
                        LicenseEnd = endDate,
                        LicenseState = string.IsNullOrEmpty(tds[7]) == false && endDate <= DateTime.Now ? tds[7] : "Действует"
                    };
                });

            logger.Log($"Успешно, получено записей со страницы: {tempResults.Count()}");
            return tempResults;
        }


    }
}

