using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System;

public class ManualConfiguration : Layer
{
  #region GameObjects
  [SerializeField]
  GameObject playerSelection = null;
  [SerializeField]
  GameObject startScreen = null;
  #endregion
  #region UI
  [SerializeField]
  InputField inIP = null;

  [SerializeField]
  Button connect = null;
  [SerializeField]
  Button backButton = null;
  #endregion

  private void OnEnable()
  {
    connect.onClick.AddListener(ConnectPressed);
    backButton.onClick.AddListener(BackPressed);
  }

  void BackPressed()
  {
    Hide();
    startScreen.GetComponent<StartScreen>().Show();
  }

  private void ConnectPressed()
  {
    Match match_ip = Regex.Match(inIP.text, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");

    if (match_ip.Success)
    {
      ConnectionData.selectedIP = inIP.text;
      connect.image.color = Color.green;

      Debug.Log("IP: " + ConnectionData.selectedIP + " : " + ConnectionData.portOutTCP);

      StartScreen.InitTCP();
      Layer.tcpClient.Send(PackageType.Connected, "");

      Hide();
      playerSelection.GetComponent<PlayerSelection>().Show();
    }
    else
    {
      connect.image.color = Color.red;
      Debug.LogWarning("Invalid Input");
    }
  }

  void Start()
  {
    Hide();
  }
}
