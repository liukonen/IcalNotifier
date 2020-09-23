using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ical.Net;
using System.Net;
using System.IO;
using Ical.Net.CalendarComponents;

namespace IcalNotifier
{
    class IcalLogic
    {

        private string _urlPath;
        private Calendar _calendar;

        public IcalLogic(string urlPath)
        {
            _urlPath = urlPath;
            Refresh();
        }

        public void Refresh()
        {
            try
            {
                if (!(_calendar is null)) { _calendar.Dispose(); }


                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(_urlPath);
                
                    request.Headers.Add("Accept-Encoding", "gzip,deflate");
                    request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                    request.AllowAutoRedirect = true;
                    request.KeepAlive = false;
                    using (WebResponse response = request.GetResponse())
                    {
                        if (!(_calendar is null)) _calendar.Dispose();
                        _calendar = Calendar.Load(response.GetResponseStream());

                    }
                 
                }
            
            catch (Exception x)
            {
                File.AppendAllText("Get.log", x.ToString());
                throw x;
            }
        }

        public CalendarEvent[] GetEvents(int MaxNumber)
        {
             var Items = from CalendarEvent X in _calendar.Events
                        where Between(DateTime.Now.AddMinutes(-1).ToUniversalTime(), DateTime.Now.AddMinutes(MaxNumber).ToUniversalTime(), X.DtStart.AsUtc)
                        select X;
            return Items.ToArray();

        }
        public CalendarEvent[] GetNextDaysEvents()
        {
            return new CalendarEvent[] { };
        }

        private static bool Between(DateTime Start, DateTime End, DateTime Date)
        { return (Start <= Date && Date <= End); }

    }
}
