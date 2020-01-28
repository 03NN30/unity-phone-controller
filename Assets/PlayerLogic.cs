using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine.UI;

public class PlayerLogic : MonoBehaviour
{
  [HideInInspector]
  public bool valid_input;
  [HideInInspector]
  public bool valid_connection;

  private string message;
  int currentLevel = 1;

  [SerializeField]
  GameObject connectionData;
  [SerializeField]
  GameObject playerSelection;
  [SerializeField]
  GameObject actualJoystick;
  [SerializeField]
  Text actionButtonText;

  [SerializeField]
  Text errorMessage;

  [SerializeField]
  Joystick joystick;
  [SerializeField]
  Button action;

  bool actionButtonPressed;
  bool active = false;
  bool actionCoolDown = false;

  [HideInInspector]
  public string role;

  // use own local IP address to identify player
  [HideInInspector]
  public string localIP;

  // accelerometer
  private bool accelerometer_enabled;
  private Vector3 prev_accelerometer = new Vector3();

  float period = 5f;
  float timeOnActionClick = 0f;

  private void OnEnable()
  {
    action.onClick.AddListener(ActionPressed);
  }

  private void ActionPressed()
  {
    timeOnActionClick = Time.time;

    if (!actionCoolDown)
    {
      actionButtonPressed = true;
    }

    actionCoolDown = true;
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

  void Start()
  {
    hide();

    joystick = FindObjectOfType<Joystick>();

    valid_connection = false;
    valid_input = false;

    action.image.color = Color.green;

    accelerometer_enabled = false;

    // accelerometer
    if (SystemInfo.supportsAccelerometer)
    {
      accelerometer_enabled = true;
    }
    else
    {
      Debug.LogError("Your device doesn't have an accelerometer");
      errorMessage.text = "This device does not have an accelerometer";
    }
  }

  void Update()
  {
    if (active)
    {
      if (playerSelection.GetComponent<PlayerSelection>().receivedMessage != null || playerSelection.GetComponent<PlayerSelection>().receivedMessage != "")
      {
        int foundLevel = playerSelection.GetComponent<PlayerSelection>().receivedMessage.IndexOf("{L(");
        if (foundLevel != -1)
          currentLevel = Int32.Parse(playerSelection.GetComponent<PlayerSelection>().receivedMessage.Substring(foundLevel + 3, 1));
      }

      if (role == "Officer" || role == "Commander")
      {
        // delete content of last message
        message = "";

        // send data for horizontal and vertical movement using the accelerometer
        if (accelerometer_enabled)
        {
          Vector3 temp = Input.acceleration;

          if (prev_accelerometer != temp)
            message += "{A" + temp.ToString() + "}";
        }

        if (currentLevel == 1)
        {
          // in here also check for role ("Commander" or "Officer") to give them different UI

          actualJoystick.SetActive(false);

          if (role == "Commander")
          {
            actionButtonText.text = "Reload";
          }
          else if (role == "Officer")
          {
            actionButtonText.text = "Fire";
          }
        }
        else if (currentLevel == 2)
        {
          // in here also check for role ("Commander" or "Officer") to give them different UI

          actualJoystick.SetActive(true);
          message += "{J(" + joystick.Horizontal + ", " + joystick.Vertical + ")}";

          if (role == "Officer")
          {
            actionButtonText.text = "Reload";
          }
          else if (role == "Commander")
          {
            actionButtonText.text = "Fire";
          }
        }

        if (actionCoolDown)
        {
          action.enabled = false;

          if (Time.time - timeOnActionClick > period)
          {
            actionCoolDown = false;
            action.image.color = Color.green / 2f;
          }
          else
          {
            action.image.color = Color.red / 2f;
          }
          
          if (Time.time - timeOnActionClick < period / 2f)
          {
            //Debug.Log("sending message button pressed");
            message += "{B(1)}";
          }
        }
        else
          action.enabled = true;

        if (actionButtonPressed)
        {
          //Debug.Log("sending message button pressed");
          //message += "{B(1)}";
          actionButtonPressed = false;
        }

        // only send message if not empty
        if (message.Length > 0 && valid_connection)
        {
          string roleNumber = "-1";
          // send joystick data
          if (role == "Commander")
            roleNumber = "1";
          else if (role == "Officer")
            roleNumber = "2";

          message += "{R(" + roleNumber + ")}";
        }

        if (message.Length > 0)
        {
          Debug.Log("{" + localIP + "}" + message);

          // send final message
          Send("{" + localIP + "}" + message);
        }
      }
    }
  }

  private void Send(string message)
  {
    try
    {
      byte[] data = Encoding.UTF8.GetBytes(message);
      playerSelection.GetComponent<PlayerSelection>().client.Send(
        data,
        data.Length,
        playerSelection.GetComponent<PlayerSelection>().remoteEndPoint
        );
    }
    catch (Exception e)
    {
      print(e.ToString());
    }
  }
}
