using System;
using System.Collections.Generic;
using System.Text;
using Ical.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Music_C_.Exceptions;
using Music_C_.Models;

namespace Music_C_.Services
{
    public class WebCalService
    {
        private readonly HttpClient client;
        public WebCalService()
        {
            client = new HttpClient();
        }

        /// <summary>
        /// An Async method to fetch all calender events from a provided URL
        /// </summary>
        /// <param name="url">A webcal link in HTTP</param>
        /// <returns>A string with all founded calender events</returns>
        public async Task<string> GetCalenderEvents(string url)
        {
            try
            {
                var calEvents = await ParseEvents(url);

                StringBuilder stringBuilder = new StringBuilder();

                foreach (var calEvent in calEvents)
                {
                    stringBuilder.AppendLine(calEvent.ToString());
                }

                return stringBuilder.ToString();
            }
            catch (WrongUrlException ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Parses the provided link into CalenderEventModels
        /// </summary>
        /// <param name="url">A webcal link in HTTP</param>
        /// <exception cref="WrongURLException"></exception>
        /// <returns>An IEnumerble of type CalenderEventModel</returns>
        private async Task<IEnumerable<CalenderEventModel>> ParseEvents(string url)
        {
            List<CalenderEventModel> events = new List<CalenderEventModel>();

            try
            {
                var respons = await client.GetStringAsync(url);

                var calender = Calendar.Load(respons);

                var calEvents = calender.Events.ToList();

                calEvents.Where(x => x.End.Date >= DateTime.Now).ToList().ForEach(x =>
                           events.Add(new CalenderEventModel
                           {
                               Title = x.Summary,
                               Description = ExtractURLFromDescription(x.Description).Item1,
                               StartDate = x.Start.AsSystemLocal,
                               EndDate = x.End.AsSystemLocal,
                               URL = ExtractURLFromDescription(x.Description).Item2
                           }));
            }
            catch (SerializationException)
            {
                throw new WrongUrlException("Could not find Calenders, Please check the URL and try again!");
            }

            return events;
        }

        /// <summary>
        /// Separets the URL from a text and return the original text minus the URl, and the URL
        /// </summary>
        /// <param name="textToExtractUrlFrom"></param>
        /// <returns>(The original text without the URL, The URL)</returns>
        private (string, string) ExtractURLFromDescription(string textToExtractUrlFrom)
        {
            Regex regex = new Regex(@"(.*)((http|https)://([\w_-]+(?:(?:\.[\w_-]+)+))([\w.,@?^=%&:/~+#-]*[\w@?^=%&/~+#-])?)(.*)");
            var matches = regex.Match(textToExtractUrlFrom);
            if (matches.Success)
            {
                string url = matches.Groups[2].Value;
                string discriptionAfterUrlExtraction = textToExtractUrlFrom.Replace(url, string.Empty);
                return (discriptionAfterUrlExtraction, url);
            }
            return (textToExtractUrlFrom, "No link available.");
        }

    }
}
