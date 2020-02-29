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
  GameObject startScreen;
  [SerializeField]
  GameObject playerSelection;
  [SerializeField]
  GameObject actualActionObject;
  [SerializeField]
  GameObject actualJoystick;
  [SerializeField]
  Text actionButtonText;

  [SerializeField]
  Text errorMessage;
  [SerializeField]
  Text messageSent;

  [SerializeField]
  Joystick joystick;
  [SerializeField]
  Button action;

  // gyroscope
  private Gyroscope gyro;
  private Quaternion gyro_rotation;
  private Quaternion prev_gyro_rotation = new Quaternion();
  private bool gyro_enabled;

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

  string wo = "WeaponsOfficer";
  string oc = "OppsCommander";
  string cpt = "Captain";

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
    gyro_enabled = false;

    // gyroscope
    if (SystemInfo.supportsGyroscope)
    {
      gyro = Input.gyro;
      gyro.enabled = true;
      gyro_enabled = true;
    }
    else
    {
      Debug.LogError("Your device doesn't have an gyroscope");
      errorMessage.text += "no gyroscope";
    }

    // accelerometer
    if (SystemInfo.supportsAccelerometer)
    {
      accelerometer_enabled = true;
    }
    else
    {
      Debug.LogError("Your device doesn't have an accelerometer");

      if (!gyro_enabled)
        errorMessage.text += " and accelerometer";
      else
        errorMessage.text += "no accelerometer";

    }
  }

  void AddGyroToMessage()
  {
    if (gyro_enabled)
    {
      message += "{G" + gyro.rotationRateUnbiased + "}";
    }
  }

  void AddAccelerometerToMessage()
  {
    // send data for horizontal and vertical movement using the accelerometer
    if (accelerometer_enabled)
    {
      Vector3 temp = Input.acceleration;

      if (prev_accelerometer != temp)
      {
        message += "{A" + temp.ToString() + "}";
      }
    }
  }

  void AddJoystickToMessage()
  {
    actualJoystick.SetActive(true);
    message += "{J(" + joystick.Horizontal + ", " + joystick.Vertical + ")}";
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

      if (role.Equals(wo) || role.Equals(oc) || role.Equals(cpt))
      {
        // delete content of last message
        message = "";

        if (currentLevel == 1)
        {
          if (role.Equals(oc))
          {
            actualJoystick.SetActive(false);
            actualActionObject.SetActive(true);
            actionButtonText.text = "Reload";
            AddAccelerometerToMessage();
          }
          else if (role.Equals(wo))
          {
            actualJoystick.SetActive(false);
            actualActionObject.SetActive(true);
            actionButtonText.text = "Fire";
            AddAccelerometerToMessage();
          }
          else if (role.Equals(cpt))
          {
            actualJoystick.SetActive(true);
            actualActionObject.SetActive(false);
            AddGyroToMessage();
          }
        }
        else if (currentLevel == 2)
        {
          actualJoystick.SetActive(false);

          if (role.Equals(oc))
          {
            actualJoystick.SetActive(true);
            actualActionObject.SetActive(false);
            AddGyroToMessage();
          }
          else if (role.Equals(wo))
          {
            actualJoystick.SetActive(false);
            actualActionObject.SetActive(true);
            actionButtonText.text = "Reload";
            AddAccelerometerToMessage();
          }
          else if (role.Equals(cpt))
          {
            actualJoystick.SetActive(false);
            actualActionObject.SetActive(true);
            actionButtonText.text = "Fire";
            AddAccelerometerToMessage();
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
          if (role.Equals(oc))
            roleNumber = "1";
          else if (role.Equals(wo))
            roleNumber = "2";
          else if (role.Equals(cpt))
            roleNumber = "3";

          message += "{R(" + roleNumber + ")}";
        }

        if (message.Length > 0)
        {
          Debug.Log("{" + localIP + "}" + message);
          messageSent.text = message;

          // send final message
          Send("{" + localIP + "}" + message);
        }
        else
          messageSent.text = "";
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
