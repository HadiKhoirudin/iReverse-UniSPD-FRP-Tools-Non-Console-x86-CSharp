using System;
using System.Linq;

namespace iReverse_UniSPD_FRP.UniSPD
{
    internal static class Checksum
    {
        private static int chksum_type = 0;
        private const int CHKSUM_TYPE_CRC16 = 1;
        private const int CHKSUM_TYPE_ADD = 2;

        private const byte HDLC_HEADER = 0x7E;
        private const byte HDLC_ESCAPE = 0x7D;

        private const int CHK_FIXZERO = 1;
        private const int CHK_ORIG = 2;

        public static void TransCodeTest()
        {
            byte[] src = uni.StringToByteArray("00 01 00 08 9F 00 00 00 00 07 34 00");
            int len = src.Length;
            byte[] dst = new byte[len];

            int transcodeMax = SpdTranscodeMax(src, len, 4);
            Console.WriteLine("Transcoded Max: " + transcodeMax);

            uint crc = SpdCrc16(0, src, (uint)len);
            Console.WriteLine("CRC16: " + crc.ToString("X4"));

            uint checksum = SpdChecksum(0, src, len, CHK_ORIG);
            Console.WriteLine("Checksum: " + checksum.ToString("X4"));
        }

        public static void set_chksum_type(string type)
        {
            if (type == "crc16")
            {
                chksum_type = CHKSUM_TYPE_CRC16;
            }
            else if (type == "add")
            {
                chksum_type = CHKSUM_TYPE_ADD;
            }
            else
            {
                Console.WriteLine("Checksum type incorrect.");
            }
        }

        public static int calc_chksum(byte[] data)
        {
            if (chksum_type == CHKSUM_TYPE_CRC16)
            {
                uint c = SpdCrc16(0, data, (uint)data.Length);
                //Console.WriteLine("CRC16 Checksum    : " & c.ToString("X4"))
                return (int)c;
            }
            else if (chksum_type == CHKSUM_TYPE_ADD)
            {
                uint c = Checksum.SpdChecksum(0, uni.parse_reverse(data), data.Length, CHK_ORIG);
                //Console.WriteLine("SPD Checksum      : " & c.ToString("X4"))
                return (int)c;
            }
            else
            {
                Console.WriteLine("Error: Checksum type is incorrect.");
                return 0;
            }
        }

        public static int SpdTranscode(ref byte[] dst, byte[] src, int len)
        {
            int i = 0;
            int a = 0;
            int n = 0;
            for (i = 0; i < len; i++)
            {
                a = src[i];
                if (a == HDLC_HEADER || a == HDLC_ESCAPE)
                {
                    if (dst != null)
                    {
                        dst[n] = HDLC_ESCAPE;
                    }
                    n += 1;
                    a = a ^ 0x20;
                }
                if (dst != null)
                {
                    dst[n] = (byte)a;
                }
                n += 1;
            }
            return n;
        }

        public static int SpdTranscodeMax(byte[] src, int len, int n)
        {
            int i = 0;
            int a = 0;
            for (i = 0; i < len; i++)
            {
                a = src[i];
                if (a == HDLC_HEADER || a == HDLC_ESCAPE)
                {
                    a = 2;
                }
                else
                {
                    a = 1;
                }
                if (n < a)
                {
                    break;
                }
                n -= a;
            }
            return i;
        }

        public static uint SpdCrc16(uint crc, byte[] src, uint len)
        {
            byte[] s = new byte[len];
            Buffer.BlockCopy(src, 0, s, 0, (int)len);

            crc = (uint)(crc & 0xFFFFU);
            int i = 0;
            while (len > 0)
            {
                crc = crc ^ ((uint)s[i] << 8);
                for (int j = 0; j <= 7; j++)
                {
                    crc = (uint)((crc << 1) ^ (((crc >> 15) != 0) ? 0x11021U : 0));
                }
                len -= 1;
                i += 1;
            }
            return crc;
        }

        public static uint SpdChecksum(uint crc, byte[] src, int len, int final)
        {
            byte[] s = uni.parse_reverse(src);

            while (len > 1)
            {
                crc += (uint)(((uint)s[1] << 8) | (uint)s[0]);
                s = s.Skip(2).ToArray();
                len -= 2;
            }

            if (len != 0)
            {
                crc += (uint)s[0];
            }

            if (final != 0)
            {
                crc = (uint)((crc >> 16) + (crc & 0xFFFF));
                crc += (uint)(crc >> 16);
                crc = ~crc & 0xFFFF;

                if (len < final)
                {
                    crc = (uint)((crc >> 8) | ((crc & 0xFF) << 8));
                }
            }

            return crc;
        }
    }
}
