using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Globalization;

public class PlayerSelection : MonoBehaviour
{
  // sending
  [HideInInspector]
  public IPEndPoint remoteEndPoint;
  [HideInInspector]
  public UdpClient client;

  [SerializeField]
  Button confirmOppsCommander;
  [SerializeField]
  Button confirmWeaponsOfficer;
  [SerializeField]
  Button confirmCaptain;

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

  private bool oppsCommanderAvailable;
  private bool weaponsOfficerAvailable;
  private bool captainAvailable;

  bool ocPressed = false;
  bool woPressed = false;
  bool cptPressed = false;
  float timeOnPressed = 0f;
  float sendRolePeriod = 2f;

  bool active = false;

  private void OnEnable()
  {
    confirmOppsCommander.onClick.AddListener(CommanderPressed);
    confirmWeaponsOfficer.onClick.AddListener(OfficerPressed);
    confirmCaptain.onClick.AddListener(CaptainPressed);
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
    ocPressed = true;
    timeOnPressed = Time.time;
    Debug.Log("Opps Commander Pressed");
    GetComponent<CanvasGroup>().alpha = 0f;
    GetComponent<CanvasGroup>().blocksRaycasts = false;
    /*
    if (oppsCommanderAvailable)
    {
      hide();
      gameScreen.GetComponent<PlayerLogic>().show();
      gameScreen.GetComponent<PlayerLogic>().role = "OppsCommander";

      // once received it will send a confirmation message back
      Send("{" + GetLocalIPAddress() + "}{R(OC)}");
    }
    */
  }

  private void OfficerPressed()
  {
    woPressed = true;
    timeOnPressed = Time.time;
    Debug.Log("Weapons Officer Pressed");
    GetComponent<CanvasGroup>().alpha = 0f;
    GetComponent<CanvasGroup>().blocksRaycasts = false;
    /*
    if (weaponsOfficerAvailable)
    {
      hide();
      gameScreen.GetComponent<PlayerLogic>().show();
      gameScreen.GetComponent<PlayerLogic>().role = "WeaponsOfficer";

      // once received it will send a confirmation message back
      Send("{" + GetLocalIPAddress() + "}{R(WO)}");
    }
    */
  }

  private void CaptainPressed()
  {
    cptPressed = true;
    timeOnPressed = Time.time;
    Debug.Log("Captain pressed");
    GetComponent<CanvasGroup>().alpha = 0f;
    GetComponent<CanvasGroup>().blocksRaycasts = false;
    /*
    if (captainAvailable)
    {
      hide();
      gameScreen.GetComponent<PlayerLogic>().show();
      gameScreen.GetComponent<PlayerLogic>().role = "Captain";

      // once received it will send a confirmation message back
      Send("{" + GetLocalIPAddress() + "}{R(CPT)}");
    }
    */
  }

  public void ResetRoles()
  {
    oppsCommanderAvailable = false;
    weaponsOfficerAvailable = false;
    captainAvailable = false;
  }

  void Start()
  {
    ResetRoles();

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

      int woPos = receivedMessage.IndexOf("WO:");
      if (woPos != -1)
      {
        string temp = receivedMessage.Substring(woPos + 3);
        temp = temp.Substring(0, temp.IndexOf(")"));

        weaponsOfficerAvailable = temp.Equals("1");
      }

      int ocPos = receivedMessage.IndexOf("OC:");
      if (ocPos != -1)
      {
        string temp = receivedMessage.Substring(ocPos + 3);
        temp = temp.Substring(0, temp.IndexOf(")"));

        oppsCommanderAvailable = temp.Equals("1");
      }

      int cptPos = receivedMessage.IndexOf("CPT:");
      if (cptPos != -1)
      {
        string temp = receivedMessage.Substring(cptPos + 4);
        temp = temp.Substring(0, temp.IndexOf(")"));

        captainAvailable = temp.Equals("1");
      }

      if (!weaponsOfficerAvailable)
        confirmWeaponsOfficer.image.color = Color.grey;
      else
        confirmWeaponsOfficer.image.color = Color.white;

      if (!oppsCommanderAvailable)
        confirmOppsCommander.image.color = Color.grey;
      else
        confirmOppsCommander.image.color = Color.white;

      if (!captainAvailable)
        confirmCaptain.image.color = Color.grey;
      else
        confirmCaptain.image.color = Color.white;

      if (ocPressed)
      {
        if (Time.time - timeOnPressed < sendRolePeriod)
        {
          Send("{" + GetLocalIPAddress() + "}{R(OC)}");
        }
        else
        {
          ocPressed = false;
          hide();
          gameScreen.GetComponent<PlayerLogic>().show();
          gameScreen.GetComponent<PlayerLogic>().role = "OppsCommander";
        }
      }
      else if (woPressed)
      {
        if (Time.time - timeOnPressed < sendRolePeriod)
        {
          Send("{" + GetLocalIPAddress() + "}{R(WO)}");
        }
        else
        {
          woPressed = false;
          hide();
          gameScreen.GetComponent<PlayerLogic>().show();
          gameScreen.GetComponent<PlayerLogic>().role = "WeaponsOfficer";
        }
      }
      else if (cptPressed)
      {
        if (Time.time - timeOnPressed < sendRolePeriod)
        {
          Send("{" + GetLocalIPAddress() + "}{R(CPT)}");
        }
        else
        {
          cptPressed = false;
          hide();
          gameScreen.GetComponent<PlayerLogic>().show();
          gameScreen.GetComponent<PlayerLogic>().role = "Captain";
        }
      }
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
