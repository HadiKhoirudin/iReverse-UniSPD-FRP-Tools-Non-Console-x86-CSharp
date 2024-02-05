using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace iReverse_UniSPD_FRP.My
{
    internal static class MyProgress
    {
        public static Stopwatch Watch = new Stopwatch();
        public static int WaktuCari = 0;
        public static int totaldo = 0;
        public static int totalchecked = 0;

        public static void Delay(double dblSecs)
        {
            DateTime.Now.AddSeconds(0.0000115740740740741);
            System.DateTime dateTime = System.DateTime.Now.AddSeconds(0.0000115740740740741);
            System.DateTime dateTime1 = dateTime.AddSeconds(dblSecs);
            while (System.DateTime.Compare(System.DateTime.Now, dateTime1) <= 0)
            {
                System.Windows.Forms.Application.DoEvents();
            }
        }

        public static string GetButtonText(object sender)
        {
            if (sender is Button)
            {
                Button btn = (Button)sender;
                return btn.Text;
            }
            return "";
        }

        public static void ProcessBar1(long Process, long total)
        {
            int val = Convert.ToInt32(Math.Round(Process * 100L / (double)total));
            if (val > 99)
            {
                val = 100;
            }
            Main.SharedUI.progressBar1.Invoke(
                (Action)(() => Main.SharedUI.progressBar1.Value = val)
            );
        }

        public static void ProcessBar1(int Process)
        {
            int val = Process;
            if (val > 99)
            {
                val = 100;
            }
            Main.SharedUI.progressBar1.Invoke(
                (Action)(() => Main.SharedUI.progressBar1.Value = val)
            );
        }

        public static string GetFileSizes(long TheSize)
        {
            string str = "";
            double DoubleBytes;
            try
            {
                long num = TheSize;
                if (num >= 1099511627776L)
                {
                    DoubleBytes = TheSize / 1099511627776.0;
                    str = $"{DoubleBytes:N2} TB";
                }
                else if (num >= 1073741824L && num <= 1099511627775L)
                {
                    DoubleBytes = TheSize / 1073741824.0;
                    str = $"{DoubleBytes:N2} GB";
                }
                else if (num >= 1048576L && num <= 1073741823L)
                {
                    DoubleBytes = TheSize / 1048576.0;
                    str = $"{DoubleBytes:N2} MB";
                }
                else if (num >= 1024L && num <= 1048575L)
                {
                    DoubleBytes = TheSize / 1024.0;
                    str = $"{DoubleBytes:N2} KB";
                }
                else if (num < 0L || num > 1023L)
                {
                    str = "";
                }
                else
                {
                    DoubleBytes = TheSize;
                    str = $"{DoubleBytes:N2} bytes";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return str;
        }
    }
}
