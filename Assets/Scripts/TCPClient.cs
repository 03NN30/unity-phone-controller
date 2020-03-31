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
    Host = host;

    Port = port;

    Roles = new List<RoleType>();

    Id = Guid.NewGuid().ToString();
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
    Debug.Log("Incoming Package: " + p.packetType);
    switch (p.packetType)
    {
      case PackageType.Selection:
        Debug.Log("Selection");
        await Task.Run(() =>
        {
          Id = p.senderId;
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

      case PackageType.Connected: case PackageType.Disconnected:
        Debug.Log("Connected | Disconnected");
        Id = p.senderId;
        var roles = (bool[])p.data[0];

        Debug.Log(roles[0] + ", " + roles[1] + ", " + roles[2]);

        PlayerSelection.oppsCommanderAvailable = roles[0];
        PlayerSelection.weaponsOfficerAvailable = roles[1];
        PlayerSelection.captainAvailable = roles[2];
        break;

      case PackageType.ServerFull:
        Debug.Log("Server Full");
        ServerFull?.Invoke(p.data[0], new EventArgs());
        break;
    }
  }
}
