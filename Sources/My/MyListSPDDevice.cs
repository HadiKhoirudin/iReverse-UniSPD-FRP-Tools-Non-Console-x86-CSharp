using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;

namespace iReverse_UniSPD_FRP.My
{
    internal static class MyListSPDDevice
    {
        public static string Brand { get; set; }
        public static string DevicesName { get; set; }
        public static string ModelName { get; set; }
        public static string Platform { get; set; }

        public static void CreateListDevice()
        {
            try
            {
                RichTextBox RichTextBoxJSON = new RichTextBox();
                RichTextBoxJSON.Text = File.ReadAllText(
                    Application.StartupPath + "\\Data\\List\\Devices.json"
                );
                Console.WriteLine(RichTextBoxJSON.Text);
                List<Info> Models = DataSource(RichTextBoxJSON.Text);
                Main.SharedUI.ListBoxview.DataSource = Models;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }

        public class Info
        {
            public string Devices { get; set; }
            public string Models { get; set; }
            public string Platform { get; set; }
            public string Conn { get; set; }
            public string Boot { get; set; }
            public string New { get; set; }

            public Info(
                string Devices,
                string Models,
                string Platform,
                string Conn,
                string Boot,
                string New
            )
            {
                this.Devices = Devices;
                this.Models = Models;
                this.Platform = Platform;
                this.Conn = Conn;
                this.Boot = Boot;
                this.New = New;
            }

            public override string ToString()
            {
                return string.Format("{0} {1}", Devices, Models);
            }
        }

        public static List<Info> DataSource(string path)
        {
            Devicelists = (List<Info>)JsonConvert.DeserializeObject<List<Info>>(path);
            List<Info> lists = new List<Info>();
            lists.Clear();

            foreach (Info inf in Devicelists)
            {
                lists.Add(
                    new Info(inf.Devices, inf.Models, inf.Platform, inf.Conn, inf.Boot, inf.New)
                );
            }

            return lists;
        }

        public static List<Info> Devicelists = new List<Info>();
    }
}
