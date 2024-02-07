using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace iReverse_UniSPD_FRP.My
{
    internal static class MyDisplay
    {
        #region Disable Sleep
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        private enum EXECUTION_STATE : uint
        {
            ES_SYSTEM_REQUIRED = 0x1,
            ES_DISPLAY_REQUIRED = 0x2,
            ES_CONTINUOUS = 0x80000000U
        }

        public static void PreventSleep()
        {
            SetThreadExecutionState(
                EXECUTION_STATE.ES_CONTINUOUS
                    | EXECUTION_STATE.ES_SYSTEM_REQUIRED
                    | EXECUTION_STATE.ES_DISPLAY_REQUIRED
            );
        }

        public static void AllowSleep()
        {
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
        }
        #endregion

        public static void Alert(string msg, iReverseCustomUI.Form_Alert.enmType type)
        {
            iReverseCustomUI.Form_Alert frm = new iReverseCustomUI.Form_Alert();
            frm.showAlert(msg, type);
        }

        public static void RichLogs(string msg, Color colour, bool isBold, bool NextLine = false)
        {
            if (Main.SharedUI.Logs.InvokeRequired)
            {
                Main.SharedUI.Logs.Invoke(
                    new Action(() =>
                    {
                        Main.SharedUI.Logs.SelectionStart = Main.SharedUI.Logs.Text.Length;
                        Color selectionColor = Main.SharedUI.Logs.SelectionColor;
                        Main.SharedUI.Logs.SelectionColor = colour;
                        if (isBold)
                        {
                            Main.SharedUI.Logs.SelectionFont = new Font(
                                Main.SharedUI.Logs.Font,
                                FontStyle.Bold
                            );
                        }
                        else
                        {
                            Main.SharedUI.Logs.SelectionFont = new Font(
                                Main.SharedUI.Logs.Font,
                                FontStyle.Regular
                            );
                        }
                        Main.SharedUI.Logs.AppendText(msg);
                        Main.SharedUI.Logs.SelectionColor = selectionColor;
                        if (NextLine)
                        {
                            if (Main.SharedUI.Logs.TextLength > 0)
                            {
                                Main.SharedUI.Logs.AppendText("\r\n");
                            }
                        }
                    })
                );
            }
            else
            {
                Main.SharedUI.Logs.SelectionStart = Main.SharedUI.Logs.Text.Length;
                Color selectionColor = Main.SharedUI.Logs.SelectionColor;
                Main.SharedUI.Logs.SelectionColor = colour;
                if (isBold)
                {
                    Main.SharedUI.Logs.SelectionFont = new Font(
                        Main.SharedUI.Logs.Font,
                        FontStyle.Bold
                    );
                }
                else
                {
                    Main.SharedUI.Logs.SelectionFont = new Font(
                        Main.SharedUI.Logs.Font,
                        FontStyle.Regular
                    );
                }
                Main.SharedUI.Logs.AppendText(msg);
                Main.SharedUI.Logs.SelectionColor = selectionColor;
                if (NextLine)
                {
                    if (Main.SharedUI.Logs.TextLength > 0)
                    {
                        Main.SharedUI.Logs.AppendText("\r\n");
                    }
                }
            }
        }

        public static void RtbClear()
        {
            if (Main.SharedUI.Logs.InvokeRequired)
            {
                Main.SharedUI.Logs.Invoke(
                    new Action(() =>
                    {
                        Main.SharedUI.Logs.Clear();
                    })
                );
            }
            else
            {
                Main.SharedUI.Logs.Clear();
            }
        }

        public static void lbl_resp(string resp)
        {
            Main.SharedUI.lbl_resp.Invoke(
                new Action(() =>
                {
                    Main.SharedUI.lbl_resp.Text = resp;
                })
            );
        }
    }
}
