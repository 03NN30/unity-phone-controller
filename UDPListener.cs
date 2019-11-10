using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UDPListener : MonoBehaviour
{
  UdpClient client;

  // do not forget to set the port inside unity
  [SerializeField]
  int port;

  string text;

  // start from unity3d
  public void Start()
  {
    UDPListener receiveObj = new UDPListener();

    Thread receiveThread = new Thread(new ThreadStart(() => {
      client = new UdpClient(port);
      while (true)
      {
        try
        {
          IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);
          byte[] data = client.Receive(ref ip);

          text = Encoding.UTF8.GetString(data);

          // print message to console
          Debug.Log(text);
        }
        catch (Exception e)
        {
          print(e.ToString());
        }
      }
    }));

    receiveThread.IsBackground = true;
    receiveThread.Start();
  }
}