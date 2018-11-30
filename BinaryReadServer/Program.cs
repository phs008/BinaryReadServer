using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BinaryReadServer
{
    class Program
    {
        private static List<byte[]> _buffer = new List<byte[]>();

        static void Main(string[] args)
        {
            var filePath = ConfigurationManager.AppSettings["filepath"];
            var encodingType = ConfigurationManager.AppSettings["encoding"];
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Console.WriteLine("Check Binary txt path");
            }
            else
            {
                TakeStringToBytebuffer(filePath,encodingType);
                AsyncEchoServer();
            }

            Console.ReadLine();
        }

        private static void TakeStringToBytebuffer(string filePath , string encodingType)
        {
            string line;
            StreamReader streamReader = new StreamReader(filePath);
            while((line = streamReader.ReadLine()) != null)
            {
                _buffer.Add(GetBytesFromHexString(line));
            }
        }

        async static Task AsyncEchoServer()
        {
            try
            {
                TcpListener listener = new TcpListener(IPAddress.Any, 1234);
                listener.Start();
                while (true)
                {
                    Console.WriteLine("Wait connection.....");
                    TcpClient tc = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
                    Console.WriteLine($"Connected : {((IPEndPoint) tc.Client.RemoteEndPoint).Address.ToString()}");
                    Task.Factory.StartNew(() => AsyncTcpProcess(tc));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        async static void AsyncTcpProcess(TcpClient client)
        {
            try
            {
                TcpClient tc = client;
                while (tc.Connected)
                {
                    NetworkStream stream = tc.GetStream();
                    for (int i = 0; i < _buffer.Count; ++i)
                    {
                        if ((i + 1) >= _buffer.Count)
                            i = 0;
                        await stream.WriteAsync(_buffer[i], 0, _buffer[i].Length).ConfigureAwait(false);
                    }
                    //await stream.WriteAsync(sendData, 0, sendData.Length).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("DisConnected");
            }
        }

        static byte[] GetBytesFromHexString(string hexStr)
        {
            var byteList = new List<byte>();

            for (int i = 0; i < hexStr.Length; i = i + 2)
            {
                byteList.Add(byte.Parse($"{hexStr[i]}{hexStr[i + 1]}", System.Globalization.NumberStyles.AllowHexSpecifier));
            }

            return byteList.ToArray();
        }
    }
}
