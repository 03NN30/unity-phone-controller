using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System;

public class ManualConfiguration : MonoBehaviour
{
  [SerializeField]
  InputField inIP;
  [SerializeField]
  InputField inPort;

  [SerializeField]
  Button connect;
  [SerializeField]
  Button backButton;

  [SerializeField]
  GameObject connectionData;
  [SerializeField]
  GameObject playerSelection;
  [SerializeField]
  GameObject gameScreen;
  [SerializeField]
  GameObject startScreen;

  public void hide()
  {
    GetComponent<CanvasGroup>().alpha = 0f;
    GetComponent<CanvasGroup>().blocksRaycasts = false;
  }

  public void show()
  {
    GetComponent<CanvasGroup>().alpha = 1f;
    GetComponent<CanvasGroup>().blocksRaycasts = true;
  }

  private void OnEnable()
  {
    connect.onClick.AddListener(ConnectPressed);
    backButton.onClick.AddListener(BackPressed);
  }

  void BackPressed()
  {
    hide();
    startScreen.GetComponent<StartScreen>().show();
  }

  private void ConnectPressed()
  {
    Match match_ip = Regex.Match(inIP.text, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
    Match match_port = Regex.Match(inPort.text, @"\b\d{1,6}\b");
    if (match_ip.Success && match_port.Success)
    {
      ConnectionData temp = connectionData.GetComponent<ConnectionData>();
      temp.selectedIP = inIP.text;
      temp.selectedPort = Int32.Parse(inPort.text);

      gameScreen.GetComponent<PlayerLogic>().valid_input = true;
      connect.image.color = Color.green;

      hide();
      playerSelection.GetComponent<PlayerSelection>().show();
    }
    else
    {
      connect.image.color = Color.red;
      Debug.LogWarning("Invalid Input");
    }
  }

  void Start()
  {
    hide();
  }
}
