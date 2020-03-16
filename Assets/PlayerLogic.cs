using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine.UI;
using System.Collections;

public class PlayerLogic : MonoBehaviour
{
  [HideInInspector]
  public bool valid_input;
  [HideInInspector]
  public bool valid_connection;

  private string message;
  int currentLevel = 1;
  bool inCave = false;
  int prevLevel = 1;
  bool playGlitch = false;
  float glitchLength = 3.0f;
  float timeAtLevelChange = 0f;

  [SerializeField]
  GameObject camera;
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

    action.image.color = Color.green / 2f;

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
      //message += "{G" + gyro.rotationRateUnbiased + "}";
      string x = gyro.attitude.x.ToString("G9");
      string y = gyro.attitude.y.ToString("G9");
      string z = gyro.attitude.z.ToString("G9");
      string w = gyro.attitude.w.ToString("G9");

      message += "{G(" + x + ", " + y + ", " + z + ", " + w + ")}";
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

  void playConstantGlitchEffect(float intensity, float scanLineJitter, float verticalJump, float horizontalShake, float colorDrift)
  {
    if (!playGlitch)
    {
      var digital = camera.GetComponent<Kino.DigitalGlitch>();
      digital.enabled = true;

      digital.intensity = intensity;

      var analog = camera.GetComponent<Kino.AnalogGlitch>();
      analog.enabled = true;

      analog.scanLineJitter = scanLineJitter;
      analog.verticalJump = verticalJump;
      analog.horizontalShake = horizontalShake;
      analog.colorDrift = colorDrift;
    }
  }

  void stopGlitchEffect()
  {
    camera.GetComponent<Kino.DigitalGlitch>().enabled = false;
    camera.GetComponent<Kino.AnalogGlitch>().enabled = false;
  }

  void playGlitchEffectOnLevelChange()
  {
    // level has changed
    if (prevLevel != currentLevel)
    {
      Debug.Log("level has been changed");

      var digital = camera.GetComponent<Kino.DigitalGlitch>();
      digital.enabled = true;

      digital.intensity = 0.637f;

      var analog = camera.GetComponent<Kino.AnalogGlitch>();
      analog.enabled = true;

      analog.scanLineJitter = 0.545f;
      analog.verticalJump = 0.216f;
      analog.horizontalShake = 0f;
      analog.colorDrift = 0.521f;

      playGlitch = true;
      prevLevel = currentLevel;
      timeAtLevelChange = Time.time;
    }

    if (playGlitch)
    {
      if (Time.time - timeAtLevelChange > glitchLength)
      {
        stopGlitchEffect();

        Debug.Log("disable glitch");
        playGlitch = false;
      }
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

        int foundInCave = playerSelection.GetComponent<PlayerSelection>().receivedMessage.IndexOf("{C(");
        if (foundInCave != -1)
          inCave = true;
        else
          inCave = false;
      }

      playGlitchEffectOnLevelChange();

      if (!playGlitch)
      {
        if ((role.Equals(wo) || role.Equals(oc) || role.Equals(cpt)))
        {
          // delete content of last message
          message = "";

          // 1: 0f, 0f, 0f, 0f, 0f
          // 2: 0.01f, 0.089f, 0.059f, 0.094f, 0.240f
          // 3: 0.05f, 0.089f, 0.059f, 0.094f, 0.240f
          // 4: 0.1f, 0.517f, 0.234f, 0.661f, 0.492f
          // 5: 0.3f, 0.517f, 0.234f, 0.661f, 0.492f

          if (currentLevel == 1 || currentLevel == 4)
          {
            if (currentLevel == 1)
              playConstantGlitchEffect(0f, 0f, 0f, 0f, 0f);
            else if (currentLevel == 4)
              playConstantGlitchEffect(0.1f, 0.517f, 0.234f, 0.661f, 0.492f);

            if (inCave)
            {
              actualJoystick.SetActive(false);
              actualActionObject.SetActive(false);
              AddGyroToMessage();
            }
            else if (role.Equals(oc))
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
              AddJoystickToMessage();
            }
          }
          else if (currentLevel == 2 || currentLevel == 5)
          {
            if (currentLevel == 2)
              playConstantGlitchEffect(0.01f, 0.089f, 0.059f, 0.094f, 0.240f);
            else if (currentLevel == 5)
              playConstantGlitchEffect(0.3f, 0.517f, 0.234f, 0.661f, 0.492f);

            actualJoystick.SetActive(false);

            if (inCave)
            {
              actualJoystick.SetActive(false);
              actualActionObject.SetActive(false);
              AddGyroToMessage();
            }
            else if (role.Equals(oc))
            {
              actualJoystick.SetActive(true);
              actualActionObject.SetActive(false);
              AddGyroToMessage();
              AddJoystickToMessage();
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
          else if (currentLevel == 3)
          {
            playConstantGlitchEffect(0.05f, 0.089f, 0.059f, 0.094f, 0.240f);
          
            actualJoystick.SetActive(false);

            if (inCave)
            {
              actualJoystick.SetActive(false);
              actualActionObject.SetActive(false);
              AddGyroToMessage();
            }
            else if (role.Equals(oc))
            {
              actualJoystick.SetActive(false);
              actualActionObject.SetActive(true);
              actionButtonText.text = "Fire";
              AddAccelerometerToMessage();
            }
            else if (role.Equals(wo))
            {
              actualJoystick.SetActive(true);
              actualActionObject.SetActive(false);
              AddGyroToMessage();
              AddJoystickToMessage();
            }
            else if (role.Equals(cpt))
            {
              actualJoystick.SetActive(false);
              actualActionObject.SetActive(true);
              actionButtonText.text = "Reload";
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
            // Debug.Log("{" + localIP + "}" + message);
            messageSent.text = message;

            // send final message
            Send("{" + localIP + "}" + message);
          }
          else
            messageSent.text = "";
        }
      }
      else
      {
        actualJoystick.SetActive(false);
        actualActionObject.SetActive(false);
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
