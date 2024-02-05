using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iReverse_UniSPD_FRP.My;

namespace iReverse_UniSPD_FRP.UniSPD
{
    internal static class uni
    {
        public static int MIDST_SIZE = 528;
        public static string PortCom = "";
        public static string response = "";
        public static bool isACK = true;
        public static bool isReadFlash = false;
        public static bool logs_on = true;
        public static bool logs_buffer = false;
        public static bool isConnectedCalibrationMode = false;

        public static bool isPartitionOperation = false;
        public static byte[] buffer;

        public static void WRITE32_LE(ref byte[] p, uint a)
        {
            p[0] = (byte)(a & 0xFF);
            p[1] = (byte)((a >> 8) & 0xFF);
            p[2] = (byte)((a >> 16) & 0xFF);
            p[3] = (byte)((a >> 24) & 0xFF);
        }

        private static byte[] translate(byte[] data)
        {
            List<byte> transdata = new List<byte>();
            transdata.Add((byte)Uni_CMD.HDLC_HEADER);
            foreach (byte b in data)
            {
                if (b == Uni_CMD.HDLC_HEADER)
                {
                    transdata.Add((byte)Uni_CMD.HDLC_ESCAPE);
                    transdata.Add(0x5E);
                }
                else if (b == Uni_CMD.HDLC_ESCAPE)
                {
                    transdata.Add((byte)Uni_CMD.HDLC_ESCAPE);
                    transdata.Add(0x5D);
                }
                else
                {
                    transdata.Add(b);
                }
            }
            transdata.Add((byte)Uni_CMD.HDLC_HEADER);
            return transdata.ToArray();
        }

        public static byte[] detranslate(byte[] data)
        {
            if (data.Length > 0)
            {
                List<byte> lst = new List<byte>(data);
                if (lst[0] != Uni_CMD.HDLC_HEADER)
                {
                    return null;
                }
                lst.RemoveAt(0);
                lst.RemoveAt(lst.Count - 1);
                int i = 0;
                List<byte> detransdata = new List<byte>();
                while (i <= (lst.Count - 1))
                {
                    if (lst[i] == Uni_CMD.HDLC_ESCAPE && lst[i + 1] == 0x5E)
                    {
                        detransdata.Add((byte)Uni_CMD.HDLC_HEADER);
                        i += 2;
                    }
                    else if (lst[i] == Uni_CMD.HDLC_ESCAPE && lst[i + 1] == 0x5D)
                    {
                        detransdata.Add((byte)Uni_CMD.HDLC_ESCAPE);
                        i += 2;
                    }
                    else
                    {
                        detransdata.Add(lst[i]);
                        i += 1;
                    }
                }
                detransdata.RemoveAt(0);
                return detransdata.ToArray();
            }
            else
            {
                return new byte[0];
            }
        }

        public static byte[] ExtractData(byte[] data)
        {
            byte[] startSequence = { 0x7E, 0x0, 0x93 };
            byte[] endSequence = { 0x7E };
            List<byte> result = new List<byte>();

            List<int> indexes = new List<int>();
            int i = 0;
            while (i < data.Length)
            {
                var index = FindSequence(data, i, startSequence);
                if (index == -1)
                {
                    break;
                }
                indexes.Add(index);
                i = index + startSequence.Length;
            }

            for (int j = 0; j < indexes.Count; j++)
            {
                int startIndex = indexes[j] + startSequence.Length + 2;
                int endIndex = data.Length;

                if (j < indexes.Count - 1)
                {
                    endIndex = indexes[j + 1] - endSequence.Length - 2;
                }

                for (int k = startIndex; k < endIndex; k++)
                {
                    result.Add(data[k]);
                }
            }

            List<byte> lst = new List<byte>(result.ToArray());
            int h = 0;
            List<byte> translate = new List<byte>();
            while (h <= (lst.Count - 1))
            {
                if (lst[h] == Uni_CMD.HDLC_ESCAPE && lst[h + 1] == 0x5E)
                {
                    translate.Add((byte)Uni_CMD.HDLC_HEADER);
                    h += 2;
                }
                else if (lst[h] == Uni_CMD.HDLC_ESCAPE && lst[h + 1] == 0x5D)
                {
                    translate.Add((byte)Uni_CMD.HDLC_ESCAPE);
                    h += 2;
                }
                else
                {
                    translate.Add(lst[h]);
                    h += 1;
                }
            }
            byte[] DataClear = translate.ToArray();
            return TakeByte(DataClear, 0, (ulong)(DataClear.Length - 11));
        }

        private static int FindSequence(byte[] data, int startIndex, byte[] sequence)
        {
            for (int i = startIndex; i <= data.Length - sequence.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < sequence.Length; j++)
                {
                    if (data[i + j] != sequence[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    return i;
                }
            }
            return -1;
        }

        public static ulong StrToSize(string str)
        {
            int shl = 0;
            ulong n = Convert.ToUInt64(str.Replace("K", "").Replace("M", "").Replace("G", ""));

            if (str.EndsWith("K"))
            {
                shl = 10;
            }
            else if (str.EndsWith("M"))
            {
                shl = 20;
            }
            else if (str.EndsWith("G"))
            {
                shl = 30;
            }
            else
            {
                throw new Exception("unknown size suffix");
            }

            if (shl != 0)
            {
                long tmp = (long)n;
                tmp >>= 63 - shl;
                if (tmp != 0 && tmp != -1)
                {
                    throw new Exception("size overflow on multiply");
                }
            }

            return (ulong)(n << shl);
        }

        public static byte[] generate_packet(int command, byte[] data = null)
        {
            if (command == (int)Uni_CMD.BSL.CMD_CHECK_BAUD)
            {
                return new byte[] { (byte)Uni_CMD.BSL.CMD_CHECK_BAUD };
            }

            List<byte> packet = new List<byte>();
            packet.AddRange(parse_reverse(BitConverter.GetBytes((ushort)command)));

            if (data != null && data.Length > 0)
            {
                packet.AddRange(parse_reverse(BitConverter.GetBytes((ushort)data.Length)));
                packet.AddRange(data);
            }
            else
            {
                packet.AddRange(BitConverter.GetBytes((ushort)0));
            }

            int chksum = Checksum.calc_chksum(packet.ToArray());
            packet.AddRange(parse_reverse(BitConverter.GetBytes((ushort)chksum)));
            byte[] transdata = translate(packet.ToArray());
            return transdata;
        }

        public static byte[] generate_packet_diag(byte[] data)
        {
            List<byte> packet = new List<byte>();

            if (data != null && data.Length > 0)
            {
                packet.AddRange(parse_reverse(BitConverter.GetBytes((ushort)data.Length)));
                packet.AddRange(data);
            }
            else
            {
                packet.AddRange(BitConverter.GetBytes((ushort)0));
            }

            int chksum = Checksum.calc_chksum(packet.ToArray());
            packet.AddRange(parse_reverse(BitConverter.GetBytes((ushort)chksum)));
            byte[] transdata = translate(packet.ToArray());
            return transdata;
        }

        public static Tuple<int, byte[], bool> parse_packet(byte[] packet)
        {
            byte[] detranslatedPacket = detranslate(packet);
            if (detranslatedPacket == null)
            {
                return new Tuple<int, byte[], bool>(0, null, false);
            }
            int command = BitConverter.ToUInt16(detranslatedPacket, 0);
            int length = BitConverter.ToUInt16(detranslatedPacket, 2);
            byte[] data = detranslatedPacket;
            int chksum = BitConverter.ToUInt16(detranslatedPacket, detranslatedPacket.Length - 2);
            bool chksumMatch = (
                Checksum.calc_chksum(
                    detranslatedPacket.Take(detranslatedPacket.Length - 2).ToArray()
                ) == chksum
            );
            return new Tuple<int, byte[], bool>(command, data, chksumMatch);
        }

        public static async Task send_data(
            byte[] data,
            int offset = 0,
            CancellationToken cancelToken = default
        )
        {
            try
            {
                cancelToken.ThrowIfCancellationRequested();
                await Main.myserial.WriteAsync(data, 0, data.Length, cancelToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static async Task<bool> send_checkbaud(CancellationToken cancelToken)
        {
            await send_data(generate_packet((int)Uni_CMD.BSL.CMD_CHECK_BAUD), 0, cancelToken);
            return isACK;
        }

        public static async Task<bool> send_connect(CancellationToken cancelToken)
        {
            await send_data(generate_packet((int)Uni_CMD.BSL.CMD_CONNECT), 0, cancelToken);
            return isACK;
        }

        public static async Task<bool> send_enable_flash(CancellationToken cancelToken)
        {
            await send_data(
                generate_packet((int)Uni_CMD.BSL.CMD_ENABLE_WRITE_FLASH),
                0,
                cancelToken
            );
            return isACK;
        }

        public static async Task<bool> send_disable_transcode(CancellationToken cancelToken)
        {
            await send_data(
                generate_packet((int)Uni_CMD.BSL.CMD_DISABLE_TRANSCODE),
                0,
                cancelToken
            );
            return isACK;
        }

        public static async Task<bool> send_start_fdl(
            int addr,
            int total_size,
            CancellationToken cancelToken
        )
        {
            await send_data(
                generate_packet(
                    (int)Uni_CMD.BSL.CMD_START_DATA,
                    parse_reverse(
                        BitConverter
                            .GetBytes(total_size)
                            .Concat(BitConverter.GetBytes(addr))
                            .ToArray()
                    )
                ),
                0,
                cancelToken
            );
            return isACK;
        }

        public static async Task send_midst(byte[] data, CancellationToken cancelToken)
        {
            isPartitionOperation = true;
            await send_data(generate_packet((int)Uni_CMD.BSL.CMD_MIDST_DATA, data));
        }

        public static async Task<bool> send_end(CancellationToken cancelToken)
        {
            isPartitionOperation = false;
            await send_data(generate_packet((int)Uni_CMD.BSL.CMD_END_DATA), 0, cancelToken);
            return isACK;
        }

        public static async Task<bool> send_exec(CancellationToken cancelToken)
        {
            await send_data(generate_packet((int)Uni_CMD.BSL.CMD_EXEC_DATA), 0, cancelToken);
            return isACK;
        }

        public static async Task<bool> send_read_midst(
            int total,
            int len,
            CancellationToken cancelToken
        )
        {
            isPartitionOperation = true;
            byte[] a = BitConverter.GetBytes(total);
            byte[] b = BitConverter.GetBytes(len);
            byte[] tb = TakeByte(a, 0, 4);
            byte[] tl = TakeByte(b, 0, 4);
            byte[] Tosend = generate_packet(
                (int)Uni_CMD.BSL.CMD_READ_MIDST,
                tb.Concat(tl).ToArray()
            );
            await send_data(Tosend, 0, cancelToken);
            return isACK;
        }

        public static async Task<bool> send_read_midst(byte[] data, CancellationToken cancelToken)
        {
            isPartitionOperation = true;
            byte[] Tosend = generate_packet((int)Uni_CMD.BSL.CMD_READ_MIDST, data);
            await send_data(Tosend, 0, cancelToken);
            return isACK;
        }

        public static async Task<bool> send_read_end(CancellationToken cancelToken)
        {
            isPartitionOperation = false;
            await send_data(generate_packet((int)Uni_CMD.BSL.CMD_READ_END), 0, cancelToken);
            return isACK;
        }

        public static async Task<bool> send_select_partition(
            string name,
            ulong size,
            bool mode64 = false,
            int cmd = 0,
            CancellationToken cancelToken = default
        )
        {
            byte[] Taken = BitConverter.GetBytes(size);

            byte[] byteA = Encoding.Unicode.GetBytes(name);
            byte[] byteC = TakeByte(Taken, 0, 4);

            int lengthA = byteA.Length;
            int lengthC = byteC.Length;

            int totalLength;
            if (mode64)
            {
                totalLength = 88;
            }
            else
            {
                totalLength = 76;
            }

            int lengthB = totalLength - (lengthA + lengthC);

            byte[] byteB = new byte[lengthB];

            for (int i = 0; i < lengthB; i++)
            {
                byteB[i] = 0;
            }

            byte[] resultBytes = new byte[lengthA + lengthB + lengthC];
            Array.Copy(byteA, 0, resultBytes, 0, lengthA);
            Array.Copy(byteB, 0, resultBytes, lengthA, lengthB);
            Array.Copy(byteC, 0, resultBytes, lengthA + lengthB, lengthC);

            byte[] Tosend;
            if (mode64)
            {
                byte[] A64 = TakeByte(resultBytes, 0, 78);
                byte[] B64 = new byte[1] { 0x1 };
                byte[] C64 = TakeByte(resultBytes, 79, 9);

                resultBytes = A64.Concat(B64).Concat(C64).ToArray();

                Console.WriteLine(
                    "Result Bytes : " + BitConverter.ToString(resultBytes).Replace("-", " ")
                );

                Tosend = generate_packet(cmd, resultBytes);
            }
            else
            {
                Tosend = generate_packet(cmd, resultBytes);
            }

            await send_data(Tosend, 0, cancelToken);

            return isACK;
        }

        public static async Task<bool> send_select_partition(
            string name,
            string size = "1M",
            bool mode64 = false,
            int cmd = 0,
            CancellationToken cancelToken = default
        )
        {
            ulong asize = StrToSize(size.ToUpper());

            byte[] Taken = BitConverter.GetBytes(asize);

            byte[] byteA = Encoding.Unicode.GetBytes(name);
            byte[] byteC = TakeByte(Taken, 0, 4);

            int lengthA = byteA.Length;
            int lengthC = byteC.Length;

            int totalLength;
            if (mode64)
            {
                totalLength = 88;
            }
            else
            {
                totalLength = 76;
            }

            int lengthB = totalLength - (lengthA + lengthC);

            byte[] byteB = new byte[lengthB];

            for (int i = 0; i < lengthB; i++)
            {
                byteB[i] = 0;
            }

            byte[] resultBytes = new byte[lengthA + lengthB + lengthC];
            Array.Copy(byteA, 0, resultBytes, 0, lengthA);
            Array.Copy(byteB, 0, resultBytes, lengthA, lengthB);
            Array.Copy(byteC, 0, resultBytes, lengthA + lengthB, lengthC);

            byte[] Tosend;
            if (mode64)
            {
                byte[] A64 = TakeByte(resultBytes, 0, 78);
                byte[] B64 = new byte[1] { 0x1 };
                byte[] C64 = TakeByte(resultBytes, 79, 9);

                resultBytes = A64.Concat(B64).Concat(C64).ToArray();

                Console.WriteLine(
                    "Result Bytes : " + BitConverter.ToString(resultBytes).Replace("-", " ")
                );

                Tosend = generate_packet(cmd, resultBytes);
            }
            else
            {
                Tosend = generate_packet(cmd, resultBytes);
            }

            await send_data(Tosend, 0, cancelToken);

            return isACK;
        }

        public static async Task<string> send_get_partition_size(
            string name,
            CancellationToken cancelToken
        )
        {
            if (name != "userdata")
            {
                string a = await send_check_partition_size("userdata", cancelToken);
                ulong userdatasize = uni.StrToSize(await send_check_partition_size(a, cancelToken));

                string b = await send_check_partition_size(name, cancelToken);
                ulong partsize = uni.StrToSize(b);

                if (userdatasize > partsize)
                {
                    return b;
                }
                else
                {
                    return "0";
                }
            }
            else
            {
                return await send_check_partition_size(name, cancelToken);
            }
        }

        public static async Task<string> send_check_partition_size(
            string name,
            CancellationToken cancelToken
        )
        {
            isPartitionOperation = true;
            uint t32;
            ulong n64;
            ulong offset = 0;
            int i,
                start = 47;

            Console.WriteLine();
            Console.WriteLine("Getting Size Of Partition : " + name);
            Console.WriteLine();

            await send_select_partition(
                name,
                "256G",
                true,
                (int)Uni_CMD.BSL.CMD_READ_START,
                cancelToken
            );

            for (i = start; i >= 20; i--)
            {
                cancelToken.ThrowIfCancellationRequested();

                uint[] data = new uint[3];
                n64 = offset + ((1UL << i) - (1 << 20));
                byte[] a = BitConverter.GetBytes(data[0]);
                byte[] b = BitConverter.GetBytes(data[1]);
                byte[] c = BitConverter.GetBytes(data[2]);
                WRITE32_LE(ref a, 4);
                WRITE32_LE(ref b, (uint)n64);
                t32 = (uint)(n64 >> 32);
                WRITE32_LE(ref c, t32);

                byte[] resultBytes = new byte[a.Length + b.Length + c.Length];
                Array.Copy(a, 0, resultBytes, 0, a.Length);
                Array.Copy(b, 0, resultBytes, a.Length, b.Length);
                Array.Copy(c, 0, resultBytes, a.Length + b.Length, c.Length);

                await send_read_midst(resultBytes, cancelToken);

                if (!isReadFlash)
                    continue;
                offset = n64 + (1 << 20);
            }
            Console.WriteLine("Partition : " + name + " Size : " + offset);
            await send_read_end(cancelToken);

            int Count = 1;
            string size;
            if (offset > 0)
            {
                do
                {
                    if (uni.StrToSize(Count.ToString() + "M") > offset)
                    {
                        size = Count - 1 + "M";
                        break;
                    }
                    Count += 1;
                } while (true);
            }
            else
            {
                size = "0";
            }
            isPartitionOperation = false;
            return size;
        }

        public static async Task send_reset(CancellationToken cancelToken)
        {
            MyDisplay.lbl_resp("Rebooting...");
            MyDisplay.RichLogs(" ", Color.Black, true, true);
            MyDisplay.RichLogs("Reboot               : ", Color.Black, true, false);
            await send_data(generate_packet((int)Uni_CMD.BSL.CMD_NORMAL_RESET), 0, cancelToken);
            MyDisplay.RichLogs("OK", Color.Lime, true, true);
            Main.SharedUI.CkFDLLoaded.Invoke(
                new Action(() =>
                {
                    Main.SharedUI.CkFDLLoaded.Checked = false;
                })
            );
            Main.myserial?.Dispose();
            MyDisplay.RichLogs(" ", Color.Black, true, true);
            MyDisplay.RichLogs(
                "__________________________________________________________________",
                Color.Black,
                true,
                true
            );
            MyDisplay.RichLogs("Task completed! ...", Color.Black, true, true);
            MyDisplay.lbl_resp("");
        }

        public static async Task send_keepcharge(CancellationToken cancelToken)
        {
            await send_data(generate_packet((int)Uni_CMD.BSL.CMD_KEEP_CHARGE), 0, cancelToken);
        }

        public static async Task<bool> read_ack(byte[] buff, CancellationToken cancelToken)
        {
            cancelToken.ThrowIfCancellationRequested();
            byte[] resp = buff;

            if (resp.Length > 0)
            {
                Tuple<int, byte[], bool> tuple = parse_packet(resp);
                int response = tuple.Item1;
                byte[] Data = tuple.Item2;
                bool chksumMatch = tuple.Item3;

                if (response == (int)Uni_CMD.BSL.REP_VER)
                {
                    string version;

                    version = Encoding.UTF8.GetString(TakeByte(Data, 3, (ulong)(Data.Length - 3)));

                    if (!String.IsNullOrEmpty(version))
                    {
                        MyDisplay.RichLogs("Boot version	     : ", Color.Black, true, false);
                        MyDisplay.RichLogs(version, Color.DarkBlue, true, true);
                        Console.WriteLine("Boot version: " + version);
                        MyDisplay.lbl_resp(version);
                    }
                }
                if (response == (int)Uni_CMD.BSL.REP_ACK)
                {
                    if (!logs_on)
                    {
                        Console.WriteLine("Response : ACK Received");
                        MyDisplay.lbl_resp("ACK Received");
                    }
                }
                if (response == (int)Uni_CMD.BSL.REP_READ_FLASH)
                {
                    if (chksumMatch)
                    {
                        if (Data != null)
                        {
                            buffer = TakeByte(Data, 3, (ulong)(Data.Length - 5));
                        }
                        else
                        {
                            buffer = null;
                        }
                    }
                    if (!logs_on)
                        MyDisplay.lbl_resp("Reading Flash Data");

                    isReadFlash = true;
                    isACK = true;
                    return true;
                }
                else
                {
                    isReadFlash = false;
                }
                if (response == (int)Uni_CMD.BSL.REP_VERIFY_ERROR)
                {
                    MyDisplay.RichLogs(
                        "Response" + "\t" + ": Verify Error",
                        Color.DarkRed,
                        true,
                        true
                    );

                    MyDisplay.lbl_resp("Verify Error");
                    isACK = false;
                    return false;
                }
                if (response == (int)Uni_CMD.BSL.REP_INCOMPATIBLE_PARTITION)
                {
                    await send_enable_flash(cancelToken);

                    MyDisplay.lbl_resp("Download Mode");
                    isACK = true;
                    return true;
                }
                if (response == (int)Uni_CMD.BSL.REP_DOWN_SIZE_ERROR)
                {
                    if (!isPartitionOperation)
                    {
                        MyDisplay.RichLogs(
                            "Response" + "\t" + ": Download Size Error!",
                            Color.DarkRed,
                            true,
                            true
                        );
                        MyDisplay.lbl_resp("Download Size Error!");
                        isACK = false;
                        return false;
                    }
                }
            }
            isACK = true;
            return true;
        }

        public static string BytesToHextring(byte[] input)
        {
            return BitConverter.ToString(input).Replace("-", "").ToLower();
        }

        public static byte[] TakeByte(byte[] source, ulong start, ulong length)
        {
            return (
                (from element in source select element).Skip((int)start).Take((int)length)
            ).ToArray();
        }

        public static byte[] parse_reverse(byte[] data)
        {
            string a = BitConverter.ToString(data).Replace("-", " ");
            byte[] b = StringToByteArray(ReverseBytes(a));
            return b;
        }

        public static string ReverseBytes(string value)
        {
            string reversed = "";
            string val = value.Replace(" ", "").Replace("-", "");
            for (int i = val.Length - 2; i >= 0; i -= 2)
            {
                reversed += val.Substring(i, 2);
            }
            return reversed;
        }

        public static byte[] StringToByteArray(string hex)
        {
            hex = hex.Replace(" ", "").Replace("-", "");

            byte[] bytes = new byte[hex.Length / 2 - 1 + 1];

            for (int i = 0; i <= bytes.Length - 1; i++)
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);

            return bytes;
        }
    }
}
