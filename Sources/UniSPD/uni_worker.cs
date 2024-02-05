using System;
using System.IO;
using System.Drawing;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;
using iReverse_UniSPD_FRP.My;
using iReverse_UniSPD_FRP.UniSPD.Method;

namespace iReverse_UniSPD_FRP.UniSPD
{
    internal static class uni_worker
    {
        public static string WorkerMethod = null;

        public static async Task UniworkerStart(CancellationToken cancelToken)
        {
            if (!String.IsNullOrEmpty(WorkerMethod))
            {
                cancelToken.ThrowIfCancellationRequested();
                MyDisplay.RichLogs("Searching USB Port   : ", Color.Black, true, false);
                while (true)
                {
                    cancelToken.ThrowIfCancellationRequested();

                    if (!String.IsNullOrEmpty(uni.PortCom))
                    {
                        break;
                    }
                }

                MyDisplay.RichLogs(
                    "Found At COM" + uni.PortCom + Environment.NewLine,
                    Color.Black,
                    true,
                    true
                );

                if (!String.IsNullOrEmpty(uni.PortCom))
                {
                    SerialPort serialPort = new SerialPort("COM" + uni.PortCom, 115200);
                    serialPort.ReadTimeout = 120000;
                    serialPort.WriteTimeout = 120000;
                    Main.myserial = new MySerialDevice(serialPort);
                    await Main.myserial.ConnectAsync();
                }

                if (Main.myserial.m_port.IsOpen)
                {
                    if (!Main.SharedUI.CkFDLLoaded.Checked)
                    {
                        await MethodDownload.ConnectDownload(cancelToken);
                        await Task.Delay(TimeSpan.FromSeconds(2.0));
                        await uni.send_keepcharge(cancelToken);
                    }

                    await UniworkerTodo(cancelToken);
                    await Task.Delay(TimeSpan.FromSeconds(2.0));
                    Main.myserial.Dispose();
                }
                else
                {
                    MyDisplay.RichLogs("Error! Can't open port.", Color.Crimson, true, true);
                }
                Main.isUniSPDRunning = false;
                return;
            }
            else
            {
                Main.isUniSPDRunning = false;
                return;
            }
        }

        public static async Task UniworkerTodo(CancellationToken cancelToken)
        {
            string method = WorkerMethod;

            Checksum.set_chksum_type("add");
            await uni.send_enable_flash(cancelToken);

            Console.WriteLine("Doing " + method);
            if (method == "RECOVERY WIPE DATA I + FRP")
            {
                string files = Application.StartupPath + "\\Data\\Misc\\1";
                await Recovery_Command(files, cancelToken);
                await Erase_FRP(cancelToken);
            }
            else if (method == "RECOVERY WIPE DATA II + FRP")
            {
                string files = Application.StartupPath + "\\Data\\Misc\\2";
                await Recovery_Command(files, cancelToken);
                await Erase_FRP(cancelToken);
            }
            else if (method == "RECOVERY FORMAT DATA + FRP")
            {
                string files = Application.StartupPath + "\\Data\\Misc\\3";
                await Recovery_Command(files, cancelToken);
                await Erase_FRP(cancelToken);
            }
            else if (method == "RECOVERY WIPE APP DATA + FRP")
            {
                string files = Application.StartupPath + "\\Data\\Misc\\4";
                await Recovery_Command(files, cancelToken);
                await Erase_FRP(cancelToken);
            }
            else if (method == "RECOVERY WIPE DATA ONLY + FRP")
            {
                string files = Application.StartupPath + "\\Data\\Misc\\5";
                await Recovery_Command(files, cancelToken);
                await Erase_FRP(cancelToken);
            }
            else if (method == "ERASE DATA + FRP")
            {
                await Erase_Data(cancelToken);
                await Erase_FRP(cancelToken);
            }
            else if (method == "ERASE FRP ONLY")
            {
                await Erase_FRP(cancelToken);
            }
            await uni.send_reset(cancelToken);
        }

        public static async Task Erase_Data(CancellationToken cancelToken)
        {
            string size = "1M";
            await ErasePartition("misc", size, cancelToken);
            await ErasePartition("userdata", size, cancelToken);
        }

        public static async Task Erase_FRP(CancellationToken cancelToken)
        {
            string size = "2M";
            await ErasePartition("persist", size, cancelToken);
        }

        public static async Task Recovery_Command(string files, CancellationToken cancelToken)
        {
            string size = "1M";
            await ErasePartition("misc", size, cancelToken);
            await FlashPartition("misc", files, cancelToken);
        }

        public static async Task FlashPartition(
            string partition,
            string location,
            CancellationToken cancelToken
        )
        {
            string size;
            byte[] PartitionData;
            ulong PartitionData_len;
            ulong PartitionData_writen = 0;
            if (partition == "misc")
            {
                MyDisplay.RichLogs("Recovery command     : ", Color.Black, true, false);
            }
            else
            {
                MyDisplay.RichLogs("Writing data         : ", Color.Black, true, false);
            }
            if (File.Exists(location))
            {
                MyProgress.ProcessBar1(0);

                PartitionData = File.ReadAllBytes(location);
                if ((ulong)PartitionData.Length < uni.StrToSize("1M"))
                {
                    PartitionData_len = (ulong)PartitionData.Length;
                }
                else
                {
                    int Count = 1;
                    do
                    {
                        if (uni.StrToSize(Count.ToString() + "M") > (ulong)PartitionData.Length)
                        {
                            size = Count - 1 + "M";
                            break;
                        }
                        Count += 1;
                    } while (true);
                    PartitionData = uni.TakeByte(
                        File.ReadAllBytes(location),
                        0,
                        uni.StrToSize(size)
                    );
                    PartitionData_len = (ulong)PartitionData.Length;
                }

                if (PartitionData_len > 0)
                {
                    await uni.send_select_partition(
                        partition,
                        PartitionData_len,
                        false,
                        (int)Uni_CMD.BSL.CMD_START_DATA,
                        cancelToken
                    );
                }
                else
                {
                    MyDisplay.RichLogs("Failed! File size overflow.", Color.Red, true, true);
                    return;
                }
            }
            else
            {
                MyDisplay.RichLogs("Failed! File Doesn't Exist", Color.Red, true, true);
                return;
            }

            while ((PartitionData_len > 0))
            {
                if (PartitionData_len > 4088)
                {
                    await uni.send_midst(
                        uni.TakeByte(PartitionData, PartitionData_writen, 4088),
                        cancelToken
                    );

                    PartitionData_len -= 4088;
                    PartitionData_writen += 4088;
                }
                else
                {
                    await uni.send_midst(
                        uni.TakeByte(PartitionData, PartitionData_writen, PartitionData_len),
                        cancelToken
                    );

                    PartitionData_writen += PartitionData_len;
                    PartitionData_len = 0;
                }

                MyProgress.ProcessBar1((long)PartitionData_writen, PartitionData.Length);
            }

            await uni.send_end(cancelToken);

            MyDisplay.RichLogs("OK", Color.Lime, true, true);
        }

        public static async Task ReadPartition(
            string partition,
            string size,
            CancellationToken cancelToken
        )
        {
            Console.WriteLine("Partition Name : " + partition + " Partition size : " + size);
            MyDisplay.RichLogs("Reading Partition " + partition + " : ", Color.Black, true, false);

            await uni.send_select_partition(
                partition,
                size,
                false,
                (int)Uni_CMD.BSL.CMD_READ_START,
                cancelToken
            );

            FileStream stream = new FileStream(
                partition + ".img",
                FileMode.Append,
                FileAccess.Write
            );
            using (stream)
            {
                byte[] buffer = new byte[4097];

                ulong i = 0;
                ulong BYTES_TO_READ = uni.StrToSize(size); // Partition Size
                ulong bytesRead = 0;
                ulong fileOffset = 0;
                do
                {
                    fileOffset = bytesRead * i;
                    await uni.send_read_midst((int)bytesRead, (int)fileOffset, cancelToken);
                    buffer = uni.buffer;
                    if (fileOffset == BYTES_TO_READ - bytesRead)
                    {
                        if (buffer != null)
                        {
                            await stream.WriteAsync(buffer, 0, buffer.Length);

                            if (uni.logs_buffer)
                                Console.WriteLine("Buffer Data : " + buffer.Length);
                        }

                        MyProgress.ProcessBar1(100);
                        stream.Flush();
                        stream.Close();
                        await uni.send_read_end(cancelToken);
                        break;
                    }

                    if (buffer != null)
                    {
                        stream.Write(buffer, 0, buffer.Length);
                        Console.WriteLine("Buffer Data : " + buffer.Length);
                    }

                    MyProgress.ProcessBar1((long)fileOffset, (long)BYTES_TO_READ);
                    fileOffset += bytesRead;
                    i += 1;
                } while (true);
            }

            MyDisplay.RichLogs("OK", Color.Lime, true, true);
        }

        public static async Task ErasePartition(
            string partition,
            string size = "1M",
            CancellationToken cancelToken = default
        )
        {
            Console.WriteLine("Partition Name : " + partition + " Partition size : " + size);

            if (partition != "misc")
            {
                MyDisplay.RichLogs("Erasing data         : ", Color.Black, true, false);
            }
            if (
                await uni.send_select_partition(
                    partition,
                    size,
                    false,
                    (int)Uni_CMD.BSL.CMD_ERASE_FLASH,
                    cancelToken
                )
            )
            {
                if (partition != "misc")
                {
                    MyDisplay.RichLogs("OK", Color.Lime, true, true);
                }
            }
            else
            {
                if (partition != "misc")
                {
                    MyDisplay.RichLogs("Failed", Color.Crimson, true, true);
                }
            }
        }
    }
}
