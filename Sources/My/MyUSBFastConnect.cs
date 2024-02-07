using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;
using System.Linq;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Management;
using System.Runtime.InteropServices;

namespace iReverse_UniSPD_FRP.My
{
    class MyUSBFastConnect
    {
        #region COM
        public static List<comInfo> listDevices = new List<comInfo>();
        public static SerialPort Ports = new SerialPort();
        public static readonly Stopwatch watch = new Stopwatch();
        public static long delta = 0;

        public static bool open = false;
        public static string vid { get; set; }
        public static string pid { get; set; }
        public static string state { get; set; }

        public class comInfo
        {
            public string name { get; set; }
            public string hwid { get; set; }
            public string comport { get; set; }
            public int type { get; set; }
        }

        public static void getcomInfo()
        {
            ManagementEventWatcher deviceWatcher = null;
            watch.Start();

            Task.Run(() =>
            {
                try
                {
                    WqlEventQuery query = new WqlEventQuery(
                        "SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2 OR EventType = 3"
                    );
                    deviceWatcher = new ManagementEventWatcher(query);
                    deviceWatcher.EventArrived += DeviceEventArrived;
                    deviceWatcher.Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
        }

        public static void DeviceEventArrived(object sender, EventArgs e)
        {
            if (watch.ElapsedMilliseconds - delta < 100)
            {
                return;
            }

            delta = watch.ElapsedMilliseconds;

            UpdateList();
        }

        public static void UpdateList()
        {
            Task.Run(() =>
            {
                List<comInfo> list = new List<comInfo>();
                try
                {
                    ManagementObjectSearcher managementObjectSearcher =
                        new ManagementObjectSearcher(
                            "Select * From Win32_POTSModem Where Status=\"OK\""
                        );
                    try
                    {
                        foreach (ManagementBaseObject item in managementObjectSearcher.Get())
                        {
                            ManagementObject managementObject = (ManagementObject)item;
                            comInfo obj = new comInfo
                            {
                                comport = managementObject
                                    .GetPropertyValue(Convert.ToString("AttachedTo"))
                                    .ToString()
                                    .Replace("COM", "")
                            };
                            object propertyValue = managementObject.GetPropertyValue("Name");
                            string obj2 = (propertyValue != null) ? propertyValue.ToString() : null;
                            object propertyValue2 = managementObject.GetPropertyValue("AttachedTo");
                            obj.name =
                                obj2
                                + " ("
                                + ((propertyValue2 != null) ? propertyValue2.ToString() : null)
                                + ")";
                            obj.hwid = managementObject
                                .GetPropertyValue(Convert.ToString("DeviceID"))
                                .ToString();
                            obj.type = 0;
                            comInfo comInfo = obj;
                            if (
                                comInfo.hwid.Contains("6860")
                                || comInfo.hwid.Contains("PID_685D")
                                    && !comInfo.hwid.Contains("A185D30")
                                || (
                                    comInfo.name.ToLower().Contains("samsung mobile")
                                    && comInfo.name.ToLower().Contains("usb modem")
                                )
                            )
                            {
                                comInfo.type = 1;
                            }
                            else if (
                                comInfo.hwid.Contains("PID_685D")
                                && comInfo.hwid.Contains("A185D30")
                            )
                            {
                                comInfo.type = 2;
                            }
                            list.Add(comInfo);
                        }
                        managementObjectSearcher.Dispose();
                    }
                    catch (COMException value)
                    {
                        Console.WriteLine(value);
                    }
                    ManagementObjectSearcher managementObjectSearcher2 =
                        new ManagementObjectSearcher(
                            "SELECT * FROM Win32_PnPEntity Where Status=\"OK\""
                        );
                    try
                    {
                        foreach (ManagementObject item2 in managementObjectSearcher2.Get())
                        {
                            if (item2["Caption"] != null)
                            {
                                string text = item2[Convert.ToString("Name")].ToString();
                                if (
                                    (
                                        text.Contains("Serial Port")
                                        || text.ToUpper().Contains("QUALCOMM")
                                        || text.ToUpper().Contains("PRELOADER")
                                        || text.ToUpper().Contains("MEDIATEK")
                                        || text.ToUpper().Contains("SPRD")
                                        || text.Contains("LGE")
                                        || text.Contains("1.0")
                                    ) && text.Contains("(COM")
                                )
                                {
                                    string text2 = item2[Convert.ToString("DeviceID")].ToString();
                                    var array = (string[])item2["HardwareID"];
                                    comInfo comInfo2 = new comInfo
                                    {
                                        name = item2[Convert.ToString("Name")].ToString(),
                                        hwid = ((array.Length == 0) ? null : array[0]),
                                        comport = betweenStrings(
                                            item2[Convert.ToString("Name")].ToString(),
                                            "(COM",
                                            ")"
                                        )
                                    };
                                    if (
                                        text2.ToLower().Contains("diagserd")
                                        && text2.ToLower().Contains("0002")
                                    )
                                    {
                                        comInfo2.type = 3;
                                    }
                                    list.Add(comInfo2);
                                }
                            }
                        }
                        managementObjectSearcher2.Dispose();
                    }
                    catch (Exception value2)
                    {
                        Console.WriteLine(value2.Message);
                    }

                    if (list.Count != listDevices.Count)
                    {
                        listDevices = list;
                        try
                        {
                            UpdatecomboPort(listDevices);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
                catch (Exception value3)
                {
                    Console.WriteLine(value3);
                }
            });
        }

        public static void UpdatecomboPort(List<comInfo> list)
        {
            var regex = string.Empty;
            if (Main.SharedUI.ComboPort.InvokeRequired)
            {
                Main.SharedUI.ComboPort.Invoke(
                    (MethodInvoker)(
                        () =>
                        {
                            if (list.Count < Main.SharedUI.ComboPort.Items.Count)
                            {
                                Main.SharedUI.ComboPort.Text = null;
                                Main.SharedUI.ComboPort.AllowDrop = false;
                            }
                            Main.SharedUI.ComboPort.Items.Clear();
                            foreach (comInfo item in list)
                            {
                                var text = "";
                                if (item.type == 1)
                                {
                                    text = "[MTP] ";
                                }
                                else if (item.type == 2)
                                {
                                    text = "[DLM] ";
                                }
                                else if (item.type == 3)
                                {
                                    text = "[DIAG] ";
                                }
                                string text2 = text + item.name;
                                Main.SharedUI.ComboPort.Items.Add(text2);
                                if (item.name.Contains("SAMSUNG") && string.IsNullOrEmpty(regex))
                                {
                                    regex = text2;
                                }
                            }
                            if (!string.IsNullOrEmpty(regex))
                            {
                                Main.SharedUI.ComboPort.SelectedItem = regex;
                            }
                            else if (list.Count > 0)
                            {
                                Main.SharedUI.ComboPort.SelectedIndex = 0;
                            }

                            if (!string.IsNullOrEmpty(Main.SharedUI.ComboPort.Text))
                            {
                                foreach (comInfo item in list)
                                {
                                    string[] hw = VID(item.hwid);
                                    Console.WriteLine(
                                        item.name
                                            + " - [VID : "
                                            + hw[0]
                                            + " PID : "
                                            + hw[1]
                                            + "] - Connected!"
                                    );
                                    vid = hw[0];
                                    pid = hw[1];
                                    state =
                                        item.name
                                        + "\n[VID : "
                                        + hw[0]
                                        + " PID : "
                                        + hw[1]
                                        + "] - Connected!";

                                    MyDisplay.Alert(
                                        state,
                                        iReverseCustomUI.Form_Alert.enmType.Success
                                    );
                                }

                                if (!string.IsNullOrEmpty(vid))
                                    open = true;
                            }
                            else
                            {
                                open = false;
                                vid = "";
                                pid = "";
                                state = state.Replace("Connected!", "Disconnected!");
                                MyDisplay.Alert(state, iReverseCustomUI.Form_Alert.enmType.Info);
                                Console.WriteLine(state);
                            }
                        }
                    )
                );
                return;
            }
            if (list.Count < Main.SharedUI.ComboPort.Items.Count)
            {
                Main.SharedUI.ComboPort.Text = null;
                Main.SharedUI.ComboPort.AllowDrop = false;
            }
            Main.SharedUI.ComboPort.Items.Clear();
            foreach (comInfo item2 in list)
            {
                Console.WriteLine("Port Name : " + item2.name + " (COM" + item2.comport + ")");
                Main.SharedUI.ComboPort.Items.Add(item2.name + " (COM" + item2.comport + ")");
                if (item2.name.Contains("SAMSUNG") && string.IsNullOrEmpty(regex))
                {
                    regex = item2.name + " (COM" + item2.comport + ")";
                }
            }
            Main.SharedUI.ComboPort.SelectedItem = regex;
        }

        public static string betweenStrings(string text, string start, string end)
        {
            var num = text.IndexOf(start) + start.Length;
            var num2 = text.IndexOf(end, num);
            if (Equals(end, ""))
            {
                return text.Substring(num);
            }
            return text.Substring(num, num2 - num);
        }

        public static string[] VID(string stream)
        {
            var array = new string[2];
            var num = stream.IndexOf("VID_");
            var text = stream.Substring(num + 4);
            array[0] = text.Substring(0, 4);
            var num2 = stream.IndexOf("PID_");
            var text2 = stream.Substring(num2 + 4);
            array[1] = text2.Substring(0, 4);
            return array;
        }

        #endregion
    }
}
