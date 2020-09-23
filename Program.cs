using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using System.Configuration;

namespace IcalNotifier
{
    class Program
    {
        private NotifyIcon notifyIcon;
        private ContextMenu notificationMenu;
        private System.Timers.Timer MinutTimer;
        private IcalLogic Logic;
        private int ToastTime = -1;
        private int poptime = -1;
        private int MaxNumber = -1;
        private NotificationLogic.MyNotificationActivator Toast = NotificationLogic.MyNotificationActivator.Invoke();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool isFirstInstance = false;
            // Please use a unique name for the mutex to prevent conflicts with other programs
            using (Mutex mtx = new Mutex(true, "SimpleWeatherTray", out isFirstInstance))
            {
                if (isFirstInstance)
                {
                    try
                    {
                        Program notificationIcon = new Program();
                        notificationIcon.notifyIcon.Visible = true;
                        GC.Collect();
                        Application.Run();
                        notificationIcon.notifyIcon.Visible = false;
                    }
                    catch (Exception x)
                    {
                        MessageBox.Show("Error: " + x.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    mtx.ReleaseMutex();
                }
                else
                {
                    GC.Collect();
                    MessageBox.Show("App appears to be running. if not, you may have to restart your machine to get it to work.");
                }
            }
        }

        public Program()
        {
            string url = ConfigurationManager.AppSettings["Url"];
            int.TryParse(ConfigurationManager.AppSettings["ToastNotificationTime"], out ToastTime);
            int.TryParse(ConfigurationManager.AppSettings["PopNotificationTime"], out poptime);
            MaxNumber = (ToastTime > poptime) ? ToastTime : poptime;

            notifyIcon = new NotifyIcon();
            //notificationMenu = new ContextMenu(InitializeMenu());
            //notifyIcon.DoubleClick += IconDoubleClick;
            ComponentResourceManager resources = new ComponentResourceManager(typeof(Program));
            notifyIcon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            notifyIcon.ContextMenu = notificationMenu;
            Logic = new IcalLogic(url);
            MinutTimer = new System.Timers.Timer(60000);
            
            MinutTimer.Elapsed += MinutTimer_Elapsed;
            MinutTimer.Start();
        }

        private void MinutTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            int Minute = DateTime.Now.Minute;
            if (Minute == 0 || (Minute % 5) == 0)
            {
                Logic.Refresh();
                if (Minute == 0) Toast.Clear();
            }

            try
            {
                var SubItems = Logic.GetEvents(MaxNumber);

                if (MaxNumber == poptime)
                {
                    NotificationLogic.OutlookLikePopup.TryAdd(SubItems);

                    if (ToastTime >= 0)
                    {
                        var NonPop = from Subitem
                                     in SubItems
                                     where Subitem.DtStart.AsUtc <= DateTime.Now.AddMinutes(ToastTime).ToUniversalTime()
                                     select Subitem;
                        Toast.TryAdd(NonPop.ToArray());
                    }
                }
                else
                {

                    Toast.TryAdd(SubItems);

                    if (poptime >= 0)
                    {
                        var NonToast = from Subitem
                                     in SubItems
                                       where Subitem.DtStart.AsUtc <= DateTime.Now.AddMinutes(ToastTime).ToUniversalTime()
                                       select Subitem;
                        NotificationLogic.OutlookLikePopup.TryAdd(NonToast.ToArray());
                    }
                }
             }
            catch (Exception x) { System.IO.File.AppendAllText("Timer.log", x.ToString()); }

        }
    }
}
