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
    public static int Main(String[] args)
    {
        StartClient();
        return 0;
    }

    public static void StartClient()
    {
        //byte[] bytes = new byte[1024];

        try
        {

            var client = new TcpClient();
            client.Connect(new IPEndPoint(IPAddress.Loopback, 23));
            var stream = client.GetStream();

            Task.Run(async () => await Listen(stream));
            Send(stream);

            stream.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
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

    static void Send(NetworkStream stream)
    {
        do
        {
            var input = Console.ReadLine() + (char)0x0D + (char)0x0A;// + (char)0x00;
            var data = Encoding.ASCII.GetBytes(input);
            stream.Write(data, 0, data.Length);
        } while (true);
    }
}