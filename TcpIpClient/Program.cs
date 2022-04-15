using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

// Client app is the one sending messages to a Server/listener.
// Both listener and client can send messages back and forth once a
// communication is established.
public class SocketClient
{
    const int OdbDevicePort = 35000;
    static readonly IPAddress OdbDeviceAddress = IPAddress.Parse("192.168.0.10");
    //static readonly IPAddress OdbDeviceAddress = IPAddress.Loopback;

    static Aptiv.DBCFiles.DBCFile BaseDbc;
    static Aptiv.DBCFiles.DBCFile Gen1PriusDbc;
    static Aptiv.DBCFiles.DBCFile Gen2PriusDbc;
    public static int Main(String[] args)
    {
        StartClient();
        return 0;
    }

    public static void StartClient()
    {
        try
        {
            /*
            using (var dbcStream = typeof(SocketClient).Assembly.GetManifestResourceStream("TcpIpClient.Resources.prius_test_battery_r5.dbc"))
            {
                Gen2PriusDbc = Aptiv.DBCFiles.DBCFile.Load(dbcStream);
            }

            using (var dbcStream = typeof(SocketClient).Assembly.GetManifestResourceStream("TcpIpClient.Resources.Prius-2010.dbc"))
            //using (var dbcStream = typeof(SocketClient).Assembly.GetManifestResourceStream("TcpIpClient.Resources.CSS-Electronics-OBD2-v1.4.dbc"))
            {
                Gen1PriusDbc = Aptiv.DBCFiles.DBCFile.Load(dbcStream);
            }

            using (var dbcStream = typeof(SocketClient).Assembly.GetManifestResourceStream("TcpIpClient.Resources.CSS-Electronics-OBD2-v1.4.dbc"))
            {
                BaseDbc = Aptiv.DBCFiles.DBCFile.Load(dbcStream);
            }
            */
            var client = new TcpClient();

            client.Connect(new IPEndPoint(OdbDeviceAddress, OdbDevicePort));
            var stream = client.GetStream();

            //InitializeElm(stream);
            Task.Run(async () => await Listen(stream));
            Send(stream);

            stream.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    static void InitializeElm(NetworkStream stream)
    {
        SendLine(stream, "ATI"); // welcome!
        SendLine(stream, "AT H1");  // show ODB headers and byte count, in responses
        SendLine(stream, "AT SH 7E0"); // use the "main ECU" header, in case it's not already in use
        SendLine(stream, "AT E0");  // echo off
    }

    static void Send(NetworkStream stream)
    {
        do
        {
            var line = Console.ReadLine();
            SendLine(stream, line);
        } while (true);
    }

    static void SendLine(NetworkStream stream, string line)
    {
        var input = line + (char)0x0D + (char)0x0A;// + (char)0x00;
        var data = Encoding.ASCII.GetBytes(input);
        stream.Write(data, 0, data.Length);
    }

    static async Task Listen(NetworkStream stream)
    {
        var data = new Byte[1024];
        var responseData = string.Empty;

        var byteCount = stream.Read(data, 0, data.Length);
        while (byteCount > 0)
        {
            responseData = Encoding.ASCII.GetString(data);
            responseData = responseData.Replace("\r", "\n");
            Console.Write(responseData);
            data = new byte[1024];
            byteCount = await stream.ReadAsync(data, 0, data.Length);
        }
    }

}