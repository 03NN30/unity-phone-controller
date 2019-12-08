using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerSelection : MonoBehaviour
{
  [SerializeField]
  Button confirmCommander;
  [SerializeField]
  Button confirmOfficer;

  [SerializeField]
  GameObject gameScreen;

  private void OnEnable()
  {
    confirmCommander.onClick.AddListener(CommanderPressed);
    confirmOfficer.onClick.AddListener(OfficerPressed);
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

  private void CommanderPressed()
  {
    hide();
    // requires implementation of a back to channel for verficiation
    gameScreen.GetComponent<PlayerLogic>().show();
  }

  private void OfficerPressed()
  {
    hide();
    // requires implementation of a back to channel for verficiation
    gameScreen.GetComponent<PlayerLogic>().show();
  }

  void Start()
  {
    hide();  
  }
}
