using System;
using System.Drawing;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using iReverse_UniSPD_FRP.UniSPD;

namespace iReverse_UniSPD_FRP.My
{
    public class MySerialDevice : IDisposable
    {
        public readonly object m_disposeLock;
        public int maxtimeout = 1000;

        public SerialPort m_port;
        private static bool isReceived = false;

        public MySerialDevice(SerialPort port)
        {
            m_disposeLock = new object();
            m_port = port;
        }

        public Task ConnectAsync()
        {
            if (m_port == null)
            {
                throw new ObjectDisposedException("MySerialDevice");
            }
            m_port.DataReceived += port_OnReceiveDatazz;
            m_port.Open();
            m_port.BaseStream.WriteTimeout = m_port.WriteTimeout;
            m_port.BaseStream.ReadTimeout = m_port.ReadTimeout;
            return Task.CompletedTask;
        }

        private static async void port_OnReceiveDatazz(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort spL = (SerialPort)sender;
            int bufSize = spL.BytesToRead;
            byte[] buf = await Main.myserial.ReadAsync(Main.cts.Token, bufSize);

            if (!await uni.read_ack(buf, Main.cts.Token))
            {
                try
                {
                    Main.cts.Cancel();
                    Main.SharedUI.CkFDLLoaded.Invoke(
                        new Action(() =>
                        {
                            Main.SharedUI.CkFDLLoaded.Checked = false;
                        })
                    );
                    Main.cts.Token.ThrowIfCancellationRequested();
                }
                catch (OperationCanceledException)
                {
                    Main.cts = new CancellationTokenSource();
                    Main.isUniSPDRunning = false;
                }
            }

            isReceived = true;
        }

        public async Task<byte[]> ReadAsync(CancellationToken cancellationToken, int len)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (m_port != null)
            {
                CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken
                );
                byte[] buffer = new byte[len];
                Task<int> readTask = m_port.BaseStream.ReadAsync(
                    buffer,
                    0,
                    buffer.Length,
                    cts.Token
                );
                Task timeoutTask = Task.Delay(m_port.ReadTimeout);
                await Task.WhenAny(readTask, timeoutTask);
                if (timeoutTask.IsCompleted && !readTask.IsCompleted)
                {
                    cts.Cancel();
                    throw new TimeoutException();
                }
                int total = (await readTask);
                var response = new byte[total];
                Array.Copy(buffer, 0, response, 0, total);

                if (uni.logs_on)
                    Console.WriteLine(
                        "Read  <- ["
                            + response.Length
                            + "] \t\t : "
                            + BitConverter.ToString(response).Replace("-", " ")
                    );

                return response;
            }
            throw new InvalidOperationException("Port not connected");
        }

        public Task WaitDataReceived(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Task t = Task.Run(() =>
            {
                int timeout = 0;
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (timeout == maxtimeout)
                        break;

                    if (isReceived)
                        break;

                    Thread.Sleep(1);
                    timeout++;
                }
            });
            t.Wait();

            return t;
        }

        public async Task WriteAsync(
            byte[] buffer,
            int offset,
            int length,
            CancellationToken cancellationToken
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (m_port != null)
            {
                isReceived = false;
                CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken
                );
                Task writeTask = m_port.BaseStream.WriteAsync(buffer, offset, length, cts.Token);
                Task timeoutTask = Task.Delay(m_port.WriteTimeout);
                await Task.WhenAny(writeTask, timeoutTask);
                if (timeoutTask.IsCompleted && !writeTask.IsCompleted)
                {
                    cts.Cancel();
                    throw new TimeoutException();
                }

                if (uni.logs_on)
                    Console.WriteLine(
                        "Write -> ["
                            + length
                            + "] \t\t : "
                            + BitConverter.ToString(buffer).Replace("-", " ")
                    );

                Task readTask = WaitDataReceived(cancellationToken);
                await Task.WhenAny(readTask);
                return;
            }
            throw new InvalidOperationException("Port not connected");
        }

        public override string ToString()
        {
            SerialPort port = m_port;
            object obj;
            if (port != null)
            {
                obj = port.PortName;
                if (obj != null)
                {
                    return (string)obj;
                }
            }
            else
            {
                obj = null;
            }
            obj = "MySerialDevice";
            return (string)obj;
        }

        public void Dispose()
        {
            lock (m_disposeLock)
            {
                if (m_port != null)
                {
                    try
                    {
                        if (m_port.IsOpen)
                            m_port.DiscardInBuffer();
                    }
                    catch { }
                    try
                    {
                        if (m_port.IsOpen)
                            m_port.DiscardOutBuffer();
                    }
                    catch { }
                    try
                    {
                        if (m_port.IsOpen)
                            m_port.Close();

                        m_port.Dispose();
                    }
                    catch { }
                    m_port = null;
                }
            }
        }
    }
}
