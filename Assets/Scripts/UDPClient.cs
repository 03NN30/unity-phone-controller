using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDPClient
{
  private readonly bool printMessageIn = false;
  private readonly bool printMessageOut = false;

  public string ReceivedMessage { get; set; }
  public string Message { get; set; }

  private IPEndPoint remoteEndPoint;
  private UdpClient udpClient;
  private bool listener;

  public UDPClient(bool listener)
  {
    Message = "";
    this.listener = listener;
  }

  public void Initiate(string host, int port)
  {
    remoteEndPoint = new IPEndPoint(IPAddress.Parse(host), port);
    udpClient = new UdpClient();

    if (listener) Listen();
  }

  public void Listen()
  {
    ReceivedMessage = "";

    Thread receiveThread = new Thread(new ThreadStart(() =>
    {
      UdpClient self = new UdpClient(ConnectionData.portInUDP);
      while (true)
      {
        try
        {
          IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);
          byte[] data = self.Receive(ref ip);

          ReceivedMessage = Encoding.UTF8.GetString(data);

          if (printMessageIn)
            Debug.Log(ReceivedMessage);
        }
        catch (Exception e)
        {
          Debug.LogError(e.Message);
        }
      }
    }))
    {
      IsBackground = true
    };

    receiveThread.Start();
  }

  public void Send()
  {
    if (printMessageOut)
      Debug.Log(Message);

    try
    {
      byte[] data = Encoding.UTF8.GetBytes(Message);
      udpClient.Send(data, data.Length, remoteEndPoint);
    }
    catch (Exception e)
    {
      Debug.LogError(e.Message);
    }

    Message = "";
  }

  public static string GetLocalIPAddress()
  {
    var host = Dns.GetHostEntry(Dns.GetHostName());
    foreach (var ip in host.AddressList)
    {
      if (ip.AddressFamily == AddressFamily.InterNetwork)
      {
        return ip.ToString();
      }
    }

    Debug.LogError("No network adapters with an IPv4 address in the system");
    throw new Exception();
  }
}
