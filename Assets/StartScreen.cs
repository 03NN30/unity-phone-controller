using UnityEngine;
using UnityEngine.UI;

public class StartScreen : MonoBehaviour
{
  [HideInInspector]
  public static TCPClient tcpClient;

  [SerializeField]
  Text gyroSupport;
  [SerializeField]
  Text accelerometerSupport;
  [SerializeField]
  Button continueButton;
  [SerializeField]
  Dropdown dropdown;

  [SerializeField]
  GameObject connectionData;

  // other canvas groups
  [SerializeField]
  GameObject manualConfiguration;
  [SerializeField]
  GameObject playerSelection;
  [SerializeField]
  GameObject gameScreen;

  private void OnEnable()
  {
    continueButton.onClick.AddListener(ConnectPressed);
  }
  
  private void ConnectPressed()
  {
    hide();

    switch (dropdown.options[dropdown.value].text)
    {
      case "Dome":
        ConnectionData.selectedIP = ConnectionData.domeIP;
        Debug.Log("Continuing with Dome [" + ConnectionData.selectedIP);
        playerSelection.GetComponent<PlayerSelection>().show();
        gameScreen.GetComponent<PlayerLogic>().valid_input = true;
        break;
      case "WE":
        ConnectionData.selectedIP = ConnectionData.weIP;
        Debug.Log("Continuing with Debug (WE) [" + ConnectionData.selectedIP);
        playerSelection.GetComponent<PlayerSelection>().show();
        gameScreen.GetComponent<PlayerLogic>().valid_input = true;
        break;
      case "SM":
        ConnectionData.selectedIP = ConnectionData.smIP;
        Debug.Log("Continuing with Debug (SM) [" + ConnectionData.selectedIP);
        playerSelection.GetComponent<PlayerSelection>().show();
        gameScreen.GetComponent<PlayerLogic>().valid_input = true;
        break;
      case "Manual":
        Debug.Log("Continuing with Manual Configuration");
        manualConfiguration.GetComponent<ManualConfiguration>().show();
        break;
    }

    initTCP();
    tcpClient.Send(PackageType.Connected, "");
  }

  void initTCP()
  {
    tcpClient = new TCPClient(ConnectionData.selectedIP, ConnectionData.portOutTCP);
    tcpClient.Initiate();
  }

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

  void Start()
  {
    Screen.sleepTimeout = SleepTimeout.NeverSleep;

    if (!SystemInfo.supportsGyroscope)
    {
      gyroSupport.text = "No gyroscope. Please try another phone.";
      gyroSupport.color = Color.red;

      continueButton.image.color = Color.grey;
    }

    if (!SystemInfo.supportsAccelerometer)
    {
      accelerometerSupport.text = "No accelerometer. Please try another phone.";
      accelerometerSupport.color = Color.red;

      continueButton.image.color = Color.grey;
    }
  }
}
