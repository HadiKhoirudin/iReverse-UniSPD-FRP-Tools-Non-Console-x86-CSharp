using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using iReverse_UniSPD_FRP.My;

namespace iReverse_UniSPD_FRP.UniSPD.Method
{
    internal static class MethodDownload
    {
        public static byte[] fdl1 = null;
        public static int fdl1_len = 0;
        public static int fdl1_addr = 0;
        public static int fdl1_writen = 0;

        public static byte[] fdl2 = null;
        public static int fdl2_len = 0;
        public static int fdl2_addr = 0;
        public static int fdl2_writen = 0;

        public static async Task ConnectDownload(CancellationToken cancelToken)
        {
            cancelToken.ThrowIfCancellationRequested();
            Checksum.set_chksum_type("crc16");

            MyDisplay.RichLogs("Send connect	     : ", Color.Black, true, false);
            MyDisplay.RichLogs("Connect command sent", Color.Black, true, true);
            if (await uni.send_checkbaud(cancelToken))
            {
                if (await uni.send_connect(cancelToken))
                {
                    #region send_file C++ FDL1

                    if (fdl1_len > 0)
                    {
                        MyProgress.totalchecked += 1;
                        MyProgress.Delay(1);
                        await uni.send_start_fdl(fdl1_addr, fdl1_len, cancelToken);

                        MyDisplay.RichLogs("Sending FDL1         : ", Color.Black, true, false);

                        fdl1_writen = 0;
                        while (fdl1_len > 0)
                        {
                            cancelToken.ThrowIfCancellationRequested();

                            MyProgress.ProcessBar1(fdl1_writen, fdl1.Length);

                            if (fdl1_len > uni.MIDST_SIZE)
                            {
                                await uni.send_midst(
                                    uni.TakeByte(fdl1, (ulong)fdl1_writen, (ulong)uni.MIDST_SIZE),
                                    cancelToken
                                );

                                fdl1_len -= uni.MIDST_SIZE;
                                fdl1_writen += uni.MIDST_SIZE;
                            }
                            else
                            {
                                await uni.send_midst(
                                    uni.TakeByte(fdl1, (ulong)fdl1_writen, (ulong)fdl1_len),
                                    cancelToken
                                );

                                fdl1_len = 0;
                            }
                        }

                        await uni.send_end(cancelToken);
                        await uni.send_exec(cancelToken);
                        MyDisplay.RichLogs("Done", Color.Purple, true, true);
                        await uni.send_connect(cancelToken);
                    }

                    #endregion

                    #region send_file C++ FDL2

                    if (fdl2_len > 0)
                    {
                        await uni.send_connect(cancelToken);

                        Checksum.set_chksum_type("add");

                        await uni.send_start_fdl(fdl2_addr, fdl2_len, cancelToken);

                        MyDisplay.RichLogs("Sending FDL2         : ", Color.Black, true, false);

                        fdl2_writen = 0;
                        while (fdl2_len > 0)
                        {
                            cancelToken.ThrowIfCancellationRequested();

                            MyProgress.ProcessBar1(fdl2_writen, fdl2.Length);

                            if (fdl2_len > uni.MIDST_SIZE)
                            {
                                await uni.send_midst(
                                    uni.TakeByte(fdl2, (ulong)fdl2_writen, (ulong)uni.MIDST_SIZE),
                                    cancelToken
                                );
                                fdl2_len -= uni.MIDST_SIZE;
                                fdl2_writen += uni.MIDST_SIZE;
                            }
                            else
                            {
                                await uni.send_midst(
                                    uni.TakeByte(fdl2, (ulong)fdl2_writen, (ulong)fdl2_len),
                                    cancelToken
                                );
                                fdl2_len = 0;
                            }
                        }

                        await uni.send_end(cancelToken);
                        await uni.send_exec(cancelToken);

                        await uni.send_enable_flash(cancelToken);
                        MyDisplay.RichLogs("Done", Color.Purple, true, true);
                        MyProgress.totaldo += 1;

                        Main.SharedUI.CkFDLLoaded.Invoke(
                            (Action)(() => Main.SharedUI.CkFDLLoaded.Checked = true)
                        );
                    }
                    else
                    {
                        return;
                    }
                    #endregion
                }
                else
                {
                    Console.WriteLine("Failed to send ping.");
                }
            }
            else
            {
                Console.WriteLine("Failed to send ping.");
            }
            MyDisplay.RichLogs(" ", Color.Black, true, true);
            return;
        }
    }
}
