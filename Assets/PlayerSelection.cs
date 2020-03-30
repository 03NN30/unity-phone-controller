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
  public static UDPClient udpClient;

  [SerializeField]
  GameObject startScreen;

  [SerializeField]
  Button confirmOppsCommander;
  [SerializeField]
  Button confirmWeaponsOfficer;
  [SerializeField]
  Button confirmCaptain;
  [SerializeField]
  Button backButton;

  [SerializeField]
  GameObject gameScreen;

  public static bool oppsCommanderAvailable = true;
  public static bool weaponsOfficerAvailable = true;
  public static bool captainAvailable = true;

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
    backButton.onClick.AddListener(BackPressed);
  }

  void BackPressed()
  {
    hide();
    startScreen.GetComponent<StartScreen>().show();
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
    StartScreen.tcpClient.Send(PackageType.Selection, RoleType.OppsCommander.ToString());

    //ocPressed = true;
    timeOnPressed = Time.time;
    Debug.Log("Opps Commander Pressed");
    GetComponent<CanvasGroup>().alpha = 0f;
    GetComponent<CanvasGroup>().blocksRaycasts = false;
  }

  private void OfficerPressed()
  {
    StartScreen.tcpClient.Send(PackageType.Selection, RoleType.WeaponsOfficer.ToString());

    //woPressed = true;
    timeOnPressed = Time.time;
    Debug.Log("Weapons Officer Pressed");
    GetComponent<CanvasGroup>().alpha = 0f;
    GetComponent<CanvasGroup>().blocksRaycasts = false;
  }

  private void CaptainPressed()
  {
    StartScreen.tcpClient.Send(PackageType.Selection, RoleType.Captain.ToString());
    
    //cptPressed = true;
    timeOnPressed = Time.time;
    Debug.Log("Captain pressed");
    GetComponent<CanvasGroup>().alpha = 0f;
    GetComponent<CanvasGroup>().blocksRaycasts = false;
  }

  public void ResetRoles()
  {
    oppsCommanderAvailable = false;
    weaponsOfficerAvailable = false;
    captainAvailable = false;
  }



  void initUDP()
  {
    udpClient = new UDPClient();
    udpClient.Initiate(ConnectionData.selectedIP, ConnectionData.portOutUDP);
    udpClient.Listen();
  }

  void Start()
  {
    ResetRoles();

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

        initUDP();
        Debug.Log("Connection Established");
      }

      if (gameScreen.GetComponent<PlayerLogic>().valid_connection)
      {
        /*
         * Once this message is sent, the server will answer by telling about the availability of all
         * possible player roles.
         */
        udpClient.Send("{" + UDPClient.GetLocalIPAddress() + "}{R(?)}");
      }

      /*
      int woPos = udpClient.ReceivedMessage.IndexOf("WO:");
      if (woPos != -1)
      {
        string temp = udpClient.ReceivedMessage.Substring(woPos + 3);
        temp = temp.Substring(0, temp.IndexOf(")"));

        weaponsOfficerAvailable = temp.Equals("1");
      }

      int ocPos = udpClient.ReceivedMessage.IndexOf("OC:");
      if (ocPos != -1)
      {
        string temp = udpClient.ReceivedMessage.Substring(ocPos + 3);
        temp = temp.Substring(0, temp.IndexOf(")"));

        oppsCommanderAvailable = temp.Equals("1");
      }

      int cptPos = udpClient.ReceivedMessage.IndexOf("CPT:");
      if (cptPos != -1)
      {
        string temp = udpClient.ReceivedMessage.Substring(cptPos + 4);
        temp = temp.Substring(0, temp.IndexOf(")"));

        captainAvailable = temp.Equals("1");
      }
      */

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
    }
  }
}
