using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class TCPClient
{
  public Socket Master { get; set; }
  public string Id { get; set; }

  public List<RoleType> Roles { get; set; }

  public TCPClient(string host, int port)
  {
    Debug.Log("Creating TCP Client");
    Host = host;

    Port = port;

    Roles = new List<RoleType>();
  }

  public string Host { get; set; }

  public int Port { get; set; }

  public bool Initiate()
  {
    IPAddress.TryParse(Host, out IPAddress adress);
    var endPoint = new IPEndPoint(adress, Port);
    Master = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    try
    {
      IAsyncResult result = Master.BeginConnect(endPoint, null, null);

      bool success = result.AsyncWaitHandle.WaitOne(2000, true);

      if (Master.Connected)
        Master.EndConnect(result);
      else
      {
        Master.Close();
        Debug.LogError("Master.Close()");
        throw new ApplicationException("Failed to connect server.");
      }
    }
    catch (Exception e)
    {
      Debug.LogError(e.Message);
      return false;
    }

    Thread thread = new Thread(DataIn);
    thread.Start();

    return true;
  }

  public event EventHandler ServerFull;

  public void Send(PackageType p, string message)
  {
    var pack = new Package(p, Id);
    pack.data.Add(message);

    Master.Send(pack.ToBytes());
  }

  public async void DataIn()
  {
    byte[] buffer;
    int readBytes;

    while (true)
    {
      try
      {
        buffer = new byte[Master.SendBufferSize];
        readBytes = Master.Receive(buffer);
        if (readBytes > 0)
        {
          Package p = new Package(buffer);
          await DataManager(p);
        }
      }
      catch (Exception)
      {
        Debug.LogError("Server is offline");
        return;
      }
    }
  }

  public async Task DataManager(Package p)
  {
    switch (p.packetType)
    {
      case PackageType.Connected:
        await Task.Run(() =>
        {
          Id = p.data[0].ToString();
          var toDisable = (RoleType)Enum.Parse(typeof(RoleType), p.data[1].ToString());

          // disable roles
          if (toDisable == RoleType.OppsCommander)
          {
            PlayerSelection.oppsCommanderAvailable = false;
          }
          else if (toDisable == RoleType.WeaponsOfficer)
          {
            PlayerSelection.weaponsOfficerAvailable = false;
          }
          else if (toDisable == RoleType.Captain)
          {
            PlayerSelection.captainAvailable = false;
          }
        });
        break;

      case PackageType.Disconnected:
          
        break;

      case PackageType.ServerFull:
        ServerFull?.Invoke(p.data[0], new EventArgs());
        break;
    }
  }
}
