using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Ical.Net.CalendarComponents;

namespace IcalNotifier.NotificationLogic
{

    public class MyNotificationActivator 
    {

        static MyNotificationActivator Me = null;
        private List<string> Events;
 
        protected MyNotificationActivator() 
        {
            Events = new List<string>();
        }


        public static MyNotificationActivator Invoke()
        {
            if (Me == null)
            {
                Me = new MyNotificationActivator();
            }
            return Me;
        }

        public void Clear()
        {
            Events.Clear();
        }

        public void TryAdd(CalendarEvent[] Items)
        {
            foreach (var item in Items)
            {
                TryAdd(item);
            }
        }
        public void TryAdd(CalendarEvent item)
        {
            if (!Events.Contains(item.Uid))
            {
                try {
                    Events.Add(item.Uid);
                    string Header = $"\"{item.Summary}\"";
                   // string Grouping = "-n GoogleCalendarEvents";

                    string LongTime = (new DateTime(item.DtStart.Ticks)).ToLocalTime().ToLongTimeString();
                    //string Message = $"\"{item.Description}\"";
                    string footer =  $"\"{LongTime}\"";


                    System.Diagnostics.Process.Start("toast", string.Join(" ", Header, footer));
                 }
        catch(Exception x) { System.IO.File.AppendAllText("Toast.log",x.ToString()); }            
            }
        }
     }
}
