using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class PlayerSelection : MonoBehaviour
{
  // sending
  [HideInInspector]
  public IPEndPoint remoteEndPoint;
  [HideInInspector]
  public UdpClient client;

  [SerializeField]
  Button confirmCommander;
  [SerializeField]
  Button confirmOfficer;

  [SerializeField]
  int portIn;
  [SerializeField]
  GameObject connectionData;
  [SerializeField]
  GameObject gameScreen;

  [SerializeField]
  bool printMessageIn;
  [SerializeField]
  bool printMessageOut;

  // receiving
  [HideInInspector]
  public string receivedMessage;

  private bool commanderAvailable;
  private bool officerAvailable;

  bool active = false;

  private void OnEnable()
  {
    confirmCommander.onClick.AddListener(CommanderPressed);
    confirmOfficer.onClick.AddListener(OfficerPressed);
  }

  public void hide()
  {
    active = false;
    GetComponent<CanvasGroup>().alpha = 0f;
    GetComponent<CanvasGroup>().blocksRaycasts = false;
  }

  public void show()
  {
    active = true;
    GetComponent<CanvasGroup>().alpha = 1f;
    GetComponent<CanvasGroup>().blocksRaycasts = true;
  }

  private void CommanderPressed()
  {
    if (commanderAvailable)
    {
      hide();
      gameScreen.GetComponent<PlayerLogic>().show();
      gameScreen.GetComponent<PlayerLogic>().role = "Commander";
      
      // once received it will send a confirmation message back
      Send("{" + GetLocalIPAddress() + "}{R(C)}");
    }
  }

  private void OfficerPressed()
  {
    if (officerAvailable)
    {
      hide();
      gameScreen.GetComponent<PlayerLogic>().show();
      gameScreen.GetComponent<PlayerLogic>().role = "Officer";
      
      // once received it will send a confirmation message back
      Send("{" + GetLocalIPAddress() + "}{R(O)}");
    }
  }

  void Start()
  {
    commanderAvailable = false;
    officerAvailable = false;

    // start listening at port
    receivedMessage = "";

    Thread receiveThread = new Thread(new ThreadStart(() =>
    {
      UdpClient self = new UdpClient(portIn);
      while (true)
      {
        try
        {
          IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);
          byte[] data = self.Receive(ref ip);

          receivedMessage = Encoding.UTF8.GetString(data);

          if (printMessageIn)
            Debug.Log(receivedMessage);
        }
        catch (Exception e)
        {
          print(e.ToString());
        }
      }
    }))
    {
      IsBackground = true
    };
    receiveThread.Start();

    hide();
  }

  void Update()
  {
    if (active)
    {
      // establishing connection
      if (gameScreen.GetComponent<PlayerLogic>().valid_input && !gameScreen.GetComponent<PlayerLogic>().valid_connection)
      {
        gameScreen.GetComponent<PlayerLogic>().valid_connection = true;

        remoteEndPoint = new IPEndPoint(
          IPAddress.Parse(connectionData.GetComponent<ConnectionData>().selectedIP),
          connectionData.GetComponent<ConnectionData>().selectedPort
        );

        client = new UdpClient();

        gameScreen.GetComponent<PlayerLogic>().localIP = GetLocalIPAddress().ToString();

        Debug.Log("Connection Established");
      }

      if (gameScreen.GetComponent<PlayerLogic>().valid_connection)
      {
        /*
         * Once this message is sent, the server will answer by telling about the availability of all
         * possible player roles.
         */
        Send("{" + GetLocalIPAddress() + "}{R(?)}");
      }

      if (receivedMessage == "{R(O:0)(C:0)}")
      {
        officerAvailable = false;
        commanderAvailable = false;
      }
      else if (receivedMessage == "{R(O:0)(C:1)}")
      {
        officerAvailable = false;
        commanderAvailable = true;
      }
      else if (receivedMessage == "{R(O:1)(C:0)}")
      {
        officerAvailable = true;
        commanderAvailable = false;
      }
      else if (receivedMessage == "{R(O:1)(C:1)}")
      {
        officerAvailable = true;
        commanderAvailable = true;
      }

      if (!officerAvailable)
        confirmOfficer.image.color = Color.grey;
      else
        confirmOfficer.image.color = Color.white;

      if (!commanderAvailable)
        confirmCommander.image.color = Color.grey;
      else
        confirmCommander.image.color = Color.white;
    }
  }

  private void Send(string message)
  {
    if (printMessageOut)
      Debug.Log(message);
    try
    {
      byte[] data = Encoding.UTF8.GetBytes(message);
      client.Send(data, data.Length, remoteEndPoint);
    }
    catch (Exception e)
    {
      print(e.ToString());
    }
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
    throw new Exception("No network adapters with an IPv4 address in the system!");
  }
}
