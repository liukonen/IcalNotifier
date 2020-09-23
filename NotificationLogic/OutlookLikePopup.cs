using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ical.Net.CalendarComponents;

namespace IcalNotifier.NotificationLogic
{
    public partial class OutlookLikePopup : Form
    {
        private static OutlookLikePopup main;
        private Dictionary<string, CalendarEvent> ActiveItems = new Dictionary<string, CalendarEvent>();
        private System.Timers.Timer T;

        protected OutlookLikePopup()
        {
          
            InitializeComponent();
            T = new System.Timers.Timer(30000);
            T.Elapsed += T_Elapsed;
            T.Start();
        }

        private void T_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            listBox1.Items.Clear();
            listBox1.Items.AddRange((from CalendarEvent vent 
                                     in ActiveItems.Values 
                                     select $"{vent.Summary} - {(vent.Start.AsUtc - DateTime.Now.ToUniversalTime()).Minutes.ToString()} Minutes").ToArray());
        }

        public static void TryAdd(CalendarEvent Item)
        { 
        if (main is null) { main = new OutlookLikePopup(); }
            main.TryAddItem(Item);
            main.LocationLabel.Text = Item.Location;
            main.TitleLabel.Text = Item.Summary;
            main.StartTimeLabel.Text = (new DateTime(Item.Start.Year, Item.Start.Month, Item.Start.Day, Item.Start.Hour, Item.Start.Minute, Item.Start.Second)).ToLongTimeString();
        }
        public static void TryAdd(CalendarEvent[] Items)
        {
            foreach (var item in Items)
            {
                TryAdd(item);
            }
        }

            private void TryAddItem(CalendarEvent item)
        {
            if (!ActiveItems.ContainsKey(item.Uid))
            {
                this.Show();
                ActiveItems.Add(item.Uid, item);
            }
        }


    }
}
