using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using iReverse_UniSPD_FRP.My;
using iReverse_UniSPD_FRP.UniSPD;
using iReverse_UniSPD_FRP.UniSPD.Method;

namespace iReverse_UniSPD_FRP
{
    public partial class Main : Form
    {
        public static Main SharedUI;
        public static bool isUniSPDRunning = false;
        public static CancellationTokenSource cts = new CancellationTokenSource();
        public static MySerialDevice myserial;

        public Main()
        {
            InitializeComponent();
            SharedUI = this;
            MyUSBFastConnect.getcomInfo();
            MyListSPDDevice.CreateListDevice();
        }

        private void comboBoxTimeout_SelectedIndexChanged(object sender, EventArgs e)
        {
            MySerialDevice.maxtimeout = Convert.ToInt32(
                Main.SharedUI.comboBoxTimeout.Text
                    .Replace("Timeout", "")
                    .Replace("-", "")
                    .Replace("ms", "")
                    .Replace(" ", "")
            );
        }

        private void ComboPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(ComboPort.Text))
            {
                Match match1 = Regex.Match(ComboPort.Text, @"\((COM\d+)\)");
                if (match1.Success)
                {
                    uni.PortCom = match1.Groups[1].Value.Replace("COM", "");
                }
            }
            else
            {
                uni.PortCom = "";
            }
        }

        private void ListBoxViewSearch_TextChanged(object sender, EventArgs e)
        {
            if (ListBoxViewSearch.Text.Length > 0)
            {
                int i = 0;
                for (i = 0; i < ListBoxview.Items.Count; i++)
                {
                    if (
                        ListBoxview
                            .GetItemText(ListBoxview.Items[i])
                            .Contains(ListBoxViewSearch.Text)
                    )
                    {
                        ListBoxview.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void ListBoxview_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (object item in ListBoxview.SelectedItems)
            {
                if (!isUniSPDRunning)
                {
                    MyListSPDDevice.Info list = item as MyListSPDDevice.Info;
                    MyListSPDDevice.DevicesName = list.Devices;
                    MyListSPDDevice.ModelName = list.Models;
                    MyListSPDDevice.Platform = list.Platform;

                    string[] Brand = list.Devices.Split(
                        " ".ToCharArray(),
                        StringSplitOptions.RemoveEmptyEntries
                    );

                    MyListSPDDevice.Brand = Brand[0];

                    MyDisplay.RtbClear();
                    MyDisplay.RichLogs(
                        "Selected : " + list.Devices + " " + list.Models + " " + list.Platform,
                        Color.Black,
                        true,
                        true
                    );
                    MethodOneClick.SPDOneClickExecModel();
                    break;
                }
            }
        }

        private void btn_STOP_Click(object sender, EventArgs e)
        {
            try
            {
                cts.Cancel();
                Thread.Sleep(3000);
                this.CkFDLLoaded.Checked = false;
                cts.Token.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                MyDisplay.RichLogs("Task Stopped", Color.Black, true, true);
                cts = new CancellationTokenSource();
                isUniSPDRunning = false;
            }
        }
    }
}
