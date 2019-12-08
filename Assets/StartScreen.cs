using UnityEngine;
using UnityEngine.UI;

public class StartScreen : MonoBehaviour
{
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

    ConnectionData temp = connectionData.GetComponent<ConnectionData>();

    switch (dropdown.options[dropdown.value].text)
    {
      case "Dome":
        Debug.Log("Continuing with Dome [" + temp.domeIP + ":5555]");
        temp.selectedIP = temp.domeIP;
        playerSelection.GetComponent<PlayerSelection>().show();
        gameScreen.GetComponent<PlayerLogic>().valid_input = true;
        break;
      case "WE":
        Debug.Log("Continuing with Debug (WE) [" + temp.weIP + ":5555]");
        temp.selectedIP = temp.weIP;
        playerSelection.GetComponent<PlayerSelection>().show();
        gameScreen.GetComponent<PlayerLogic>().valid_input = true;
        break;
      case "SM":
        Debug.Log("Continuing with Debug (SM) [" + temp.smIP + ":5555]");
        temp.selectedIP = temp.smIP;
        playerSelection.GetComponent<PlayerSelection>().show();
        gameScreen.GetComponent<PlayerLogic>().valid_input = true;
        break;
      case "Manual":
        Debug.Log("Continuing with Manual Configuration");
        manualConfiguration.GetComponent<ManualConfiguration>().show();
        break;
    }
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
  }
}
