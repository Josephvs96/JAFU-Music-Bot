using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music_C_.Models
{
    public class CalenderEventModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string URL { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Title: {Title}");
            stringBuilder.AppendLine($"Description: {Description}");
            stringBuilder.AppendLine($"Starts on: {StartDate.ToString("dddd 'den' dd MMMM yyyy - HH:mm")}");
            stringBuilder.AppendLine($"Ends on: {EndDate.ToString("dddd 'den' dd MMMM yyyy - HH:mm")}");
            stringBuilder.AppendLine($"Link: {URL}");

            return stringBuilder.ToString();
        }
    }
}
