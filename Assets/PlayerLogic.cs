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

  [SerializeField]
  GameObject connectionData;
  [SerializeField]
  GameObject playerSelection;

  [SerializeField]
  Text errorMessage;

  [SerializeField]
  Joystick joystick;
  [SerializeField]
  Button action;

  bool actionButtonPressed;
  bool active = false;

  [HideInInspector]
  public string role;

  // use own local IP address to identify player
  [HideInInspector]
  public string localIP;

  // accelerometer
  private bool accelerometer_enabled;
  private Vector3 prev_accelerometer = new Vector3();

  private void OnEnable()
  {
    action.onClick.AddListener(ActionPressed);
  }

  private void ActionPressed()
  {
    actionButtonPressed = true;
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

        // only send message if not empty
        if (message.Length > 0 && valid_connection)
        {
          string roleNumber = "-1";
          // send joystick data
          if (role == "Commander")
            roleNumber = "1";
          else if (role == "Officer")
            roleNumber = "2";

          message += "{R(" + roleNumber + ")}{J(" + joystick.Horizontal + "," + joystick.Vertical + ")}";

          if (actionButtonPressed)
          {
            actionButtonPressed = false;
            message += "{B(1)}";
          }

          /*
           * TODO: Add fire button (pay attention to reload and fire button)
           *       Add TCP or similar like channel for communicating backwards
           *       maybe button requires TCP for higher percision
           */

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
