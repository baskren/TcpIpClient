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
            /*
            // Connect to a Remote server
            // Get Host IP Address that is used to establish a connection
            // In this case, we get one IP address of localhost that is IP : 127.0.0.1
            // If a host has multiple addresses, you will get a list of addresses
            IPHostEntry host = Dns.GetHostEntry("localhost");
            //IPAddress ipAddress = host.AddressList[0];
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 23);

            // Create a TCP/IP  socket.
            //Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sender.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.TypeOfService, 0x03);

            // Connect the socket to the remote endpoint. Catch any errors.
            try
            {
                // Connect to Remote EndPoint
                sender.Connect(remoteEP);

                Console.WriteLine($"Socket connected to {sender.RemoteEndPoint}");

                // Encode the data string into a byte array.
                
                byte[] msg = Encoding.ASCII.GetBytes("ATI");
                var msgx = new List<byte>(msg);
                msgx.Add(0x0D);
                msgx.Add(0x0A);
                //msgx.Add(0);
                msg = msgx.ToArray();

                // Send the data through the socket.
                int bytesSent = sender.Send(msg);

                // Receive the response from the remote device.
                int bytesRec = sender.Receive(bytes);
                Console.WriteLine($"Echoed test = [{Encoding.ASCII.GetString(bytes, 0, bytesRec)}]\n");

                // Release the socket.
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
 
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }
            */


            var client = new TcpClient();
            client.Connect(new IPEndPoint(IPAddress.Loopback, 23));
            var stream = client.GetStream();

            /*
            byte[] msg = Encoding.ASCII.GetBytes("ATI");
            var msgx = new List<byte>(msg);
            msgx.Add(0x0D);
            msgx.Add(0x0A);
            //msgx.Add(0);
            msg = msgx.ToArray();
            stream.Write(msg, 0, msg.Length);
            Console.WriteLine($"SENT: [{Encoding.ASCII.GetString(msg)}]");
            
            var data = new Byte[1024];
            var responseData = string.Empty;

            var byteCount = stream.Read(data, 0, data.Length);
            while (byteCount > 0)
            {
                responseData = Encoding.ASCII.GetString(data);
                //Console.WriteLine($"bytes recv: [{byteCount}][{responseData}]");
                Console.WriteLine($"========= RECV START ========= \n\n {responseData}  \n\n =========== RECV END  ==========");
                byteCount = 0;
                byteCount = stream.Read(data, 0, data.Length);
            }
            */

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
            //responseData = responseData.Replace("\r\n", "\n");
            responseData = responseData.Replace("\r", "\n");
            //responseData = responseData.Replace("\n\n", "\n");
            Console.Write(responseData);
            /*
            foreach (var charx in responseData)
                if (charx >= 10 && charx < 128)
                Console.WriteLine($"[{charx}][{(int)charx}]");
            */
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