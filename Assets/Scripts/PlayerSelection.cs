using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class PlayerSelection : Layer
{
  #region GameObjects
  [SerializeField]
  GameObject startScreen;
  [SerializeField]
  GameObject gameScreen;
  #endregion
  #region UI
  [SerializeField]
  Button confirmOppsCommander;
  [SerializeField]
  Button confirmWeaponsOfficer;
  [SerializeField]
  Button confirmCaptain;
  [SerializeField]
  Button backButton;
  #endregion

  public static bool oppsCommanderAvailable = true;
  public static bool weaponsOfficerAvailable = true;
  public static bool captainAvailable = true;

  private void OnEnable()
  {
    confirmOppsCommander.onClick.AddListener(CommanderPressed);
    confirmWeaponsOfficer.onClick.AddListener(OfficerPressed);
    confirmCaptain.onClick.AddListener(CaptainPressed);
    backButton.onClick.AddListener(BackPressed);
  }

  void BackPressed()
  {
    Hide();
    Layer.tcpClient.Send(PackageType.Disconnected, "");
    startScreen.GetComponent<StartScreen>().Show();
  }

  private void CommanderPressed()
  {
    Layer.tcpClient.Send(PackageType.Selection, RoleType.OppsCommander.ToString());
    Layer.role = RoleType.OppsCommander;
    Continue();
  }

  private void OfficerPressed()
  {
    Layer.tcpClient.Send(PackageType.Selection, RoleType.WeaponsOfficer.ToString());
    Layer.role = RoleType.WeaponsOfficer;
    Continue();
  }

  private void CaptainPressed()
  {
    Layer.tcpClient.Send(PackageType.Selection, RoleType.Captain.ToString());
    Layer.role = RoleType.Captain;
    Continue();
  }

  public void ResetRoles()
  {
    oppsCommanderAvailable = false;
    weaponsOfficerAvailable = false;
    captainAvailable = false;
  }

  void initUDP()
  {
    udpClient = new UDPClient(true);
    udpClient.Initiate(ConnectionData.selectedIP, ConnectionData.portOutUDP);
  }

  void Continue()
  {
    initUDP();
    Hide();
    gameScreen.GetComponent<PlayerLogic>().Show();
  }

  void Start()
  {
    ResetRoles();
    Hide();
  }

  void Update()
  {
    if (active)
    {
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
