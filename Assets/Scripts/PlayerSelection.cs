using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.Video;

public class PlayerSelection : Layer
{
  #region GameObjects
  [SerializeField]
  GameObject startScreen = null;
  [SerializeField]
  GameObject gameScreen = null;
  #endregion
  #region UI
  [SerializeField]
  Button confirmOppsCommander = null;
  [SerializeField]
  Button confirmWeaponsOfficer = null;
  [SerializeField]
  Button confirmCaptain = null;
  [SerializeField]
  Button backButton = null;
  [SerializeField]
  private Image blackScreen = null;
  [SerializeField]
  private VideoPlayer particles = null;
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
    Layer.Role = RoleType.OppsCommander;
    Continue();
  }

  private void OfficerPressed()
  {
    Layer.tcpClient.Send(PackageType.Selection, RoleType.WeaponsOfficer.ToString());
    Layer.Role = RoleType.WeaponsOfficer;
    Continue();
  }

  private void CaptainPressed()
  {
    Layer.tcpClient.Send(PackageType.Selection, RoleType.Captain.ToString());
    Layer.Role = RoleType.Captain;
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

    blackScreen.GetComponent<Image>().enabled = false;
    particles.Play();
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
