using UnityEngine;
using UnityEngine.UI;

public class StartScreen : Layer
{
  #region GameObjects
  [SerializeField]
  GameObject manualConfiguration;
  [SerializeField]
  GameObject playerSelection;
  [SerializeField]
  GameObject gameScreen;
  #endregion
  #region UI
  [SerializeField]
  Text gyroSupport;
  [SerializeField]
  Text accelerometerSupport;
  [SerializeField]
  Button continueButton;
  [SerializeField]
  Dropdown dropdown;
  #endregion

  private void OnEnable()
  {
    continueButton.onClick.AddListener(ConnectPressed);
  }
  
  private void ConnectPressed()
  {
    Hide();

    switch (dropdown.options[dropdown.value].text)
    {
      case "Dome":
        ConnectionData.selectedIP = ConnectionData.domeIP;
        Debug.Log("Continuing with Dome [" + ConnectionData.selectedIP);
        playerSelection.GetComponent<PlayerSelection>().Show();
        break;

      case "WE":
        ConnectionData.selectedIP = ConnectionData.weIP;
        Debug.Log("Continuing with Debug (WE) [" + ConnectionData.selectedIP);
        playerSelection.GetComponent<PlayerSelection>().Show();
        break;

      case "SM":
        ConnectionData.selectedIP = ConnectionData.smIP;
        Debug.Log("Continuing with Debug (SM) [" + ConnectionData.selectedIP);
        playerSelection.GetComponent<PlayerSelection>().Show();
        break;

      case "Manual":
        Debug.Log("Continuing with Manual Configuration");
        manualConfiguration.GetComponent<ManualConfiguration>().Show();
        break;
    }

    InitTCP();
    Layer.tcpClient.Send(PackageType.Connected, "");
  }

  void InitTCP()
  {
    Layer.tcpClient = new TCPClient(ConnectionData.selectedIP, ConnectionData.portOutTCP);
    Layer.tcpClient.Initiate();
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
    else
    {
      Layer.gyroscope = Input.gyro;
      Layer.gyroscope.enabled = true;
    }

    if (!SystemInfo.supportsAccelerometer)
    {
      accelerometerSupport.text = "No accelerometer. Please try another phone.";
      accelerometerSupport.color = Color.red;

      continueButton.image.color = Color.grey;
    }
    else
    {
      Layer.accelerometer = true;
    }
  }
}
