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
  Text actionButtonText;

  [SerializeField]
  Text errorMessage;
  [SerializeField]
  Text messageSent;

  [SerializeField]
  Button action;
  [SerializeField]
  Sprite redSprite;
  [SerializeField]
  Sprite greenSprite;
  [SerializeField]
  Button disconnectButton;

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

  [SerializeField]
  Button initCalibrationButton;
  float timeToWaitForCalibrationStep = 2f;

  [SerializeField]
  Text phoneStraightInstructions;
  [SerializeField]
  Button confirmPhoneStraightButton;
  bool confirmPhoneStraight = false;
  float timeOnConfirmPhoneStraight = 0f;

  [SerializeField]
  Text flashlightStraightInstructions;
  [SerializeField]
  Button confirmFlashlightStraightButton;
  bool confirmFlashLightStraight = false;
  float timeOnConfirmFlashlightStraight = 0f;

  [SerializeField]
  bool printMessageOut = false;

  bool pressedDisconnect = false;
  float timeOnDisconnect = 0f;
  float timeToSendDisconnectMessage = 2f;

  private void OnEnable()
  {
    action.onClick.AddListener(ActionPressed);
    initCalibrationButton.onClick.AddListener(initCalibrationPressed);
    
    confirmPhoneStraightButton.onClick.AddListener(confirmPhoneStraightPressed);
    confirmFlashlightStraightButton.onClick.AddListener(confirmFlashlightStraightPressed);

    disconnectButton.onClick.AddListener(ConfirmDisconnectButton);
  }

  private void ConfirmDisconnectButton()
  {
    pressedDisconnect = true;
    timeOnDisconnect = Time.time;
    hide();
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

  void setPhoneStraightUI(bool visible)
  {
    confirmPhoneStraightButton.gameObject.SetActive(visible);
    phoneStraightInstructions.enabled = visible;
  }

  void setFlashlightStraightUI(bool visible)
  {
    confirmFlashlightStraightButton.gameObject.SetActive(visible);
    flashlightStraightInstructions.enabled = visible;
  }

  private void initCalibrationPressed()
  {
    confirmPhoneStraightButton.gameObject.SetActive(!confirmPhoneStraightButton.gameObject.active);
    phoneStraightInstructions.enabled = !phoneStraightInstructions.enabled;

    setFlashlightStraightUI(false);
  }

  private void confirmPhoneStraightPressed()
  {
    setPhoneStraightUI(false);
    setFlashlightStraightUI(true);

    confirmPhoneStraight = true;
    timeOnConfirmPhoneStraight = Time.time;
  }

  private void confirmFlashlightStraightPressed()
  {
    if (!confirmPhoneStraight)
    {
      setPhoneStraightUI(false);
      setFlashlightStraightUI(false);

      confirmFlashLightStraight = true;
      timeOnConfirmFlashlightStraight = Time.time;

      initCalibrationButton.image.color = Color.green;
    }
  }

  public void hide()
  {
    active = false;
    GetComponent<CanvasGroup>().alpha = 0f;
    GetComponent<CanvasGroup>().blocksRaycasts = false;

    stopGlitchEffect();
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

    setPhoneStraightUI(false);
    setFlashlightStraightUI(false);

    //joystick = FindObjectOfType<Joystick>();

    valid_connection = false;
    valid_input = false;

    actualActionObject.GetComponent<Image>().sprite = greenSprite;

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
    if (confirmPhoneStraight)
    {
      //Debug.Log("ok bro");
      if (Time.time - timeOnConfirmPhoneStraight > timeToWaitForCalibrationStep)
      {
        Debug.Log("Calibration: Phone straight off");
        confirmPhoneStraight = false;
      }
      string message_t = "{C(P)}";
      Send("{" + localIP + "}" + message_t + "{R(3)}");
    }
    else if (confirmFlashLightStraight)
    {
      if (Time.time - timeOnConfirmFlashlightStraight > timeToWaitForCalibrationStep)
      {
        Debug.Log("Calibration: Flashlight straight off");
        confirmFlashLightStraight = false;
      }
      string message_t = "{C(F)}";
      Send("{" + localIP + "}" + message_t + "{R(3)}");
    }

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
    //message += "{J(" + joystick.Horizontal + ", " + joystick.Vertical + ")}";
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

  bool sendVerificationMessage = true;
  float timeTracker = 0f;
  float timeSinceVerificationInit = 0f;
  float timeToSendVerification = 2.5f;

  void Update()
  {
    string correctRoleString = "";
    if (role.Equals(wo))
      correctRoleString = "2";
    else if (role.Equals(oc))
      correctRoleString = "1";
    else if (role.Equals(cpt))
      correctRoleString = "3";

    if (pressedDisconnect)
    {
      if (Time.time - timeOnDisconnect < timeToSendDisconnectMessage)
      {
        if (!role.Equals(""))
        {
          Send("{" + localIP + "}" + "{R(" + correctRoleString + ")}{D(" + correctRoleString + ")}");
        }

      }
      else
      {
        pressedDisconnect = false;
        playerSelection.GetComponent<PlayerSelection>().ResetRoles();
        playerSelection.GetComponent<PlayerSelection>().show();
      }
    }

    if (active)
    {
      // send message every 5 seconds for 2.5 seconds to check that this client is still online
      if (Time.time > timeTracker)
      {
        timeTracker += 5f;
        sendVerificationMessage = true;
        timeSinceVerificationInit = Time.time;
      }

      if (sendVerificationMessage)
      {
        Send("{" + localIP + "}" + "{R(" + correctRoleString + ")}{P(" + correctRoleString + ")}");

        if (Time.time - timeSinceVerificationInit > timeToSendVerification)
        {
          sendVerificationMessage = false;
        }
      }
    
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

      if (role.Equals(wo) || role.Equals(oc))
        initCalibrationButton.gameObject.SetActive(false);
      else
        initCalibrationButton.gameObject.SetActive(true);

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

          if (currentLevel == 1)
          {
            playConstantGlitchEffect(0f, 0f, 0f, 0f, 0f);
            
            if (inCave)
            {
              actualActionObject.SetActive(false);
              AddGyroToMessage();
            }
            else if (role.Equals(oc))
            {
              actualActionObject.SetActive(false);
              actionButtonText.text = "Reload";
              AddAccelerometerToMessage();
            }
            else if (role.Equals(wo))
            {
              actualActionObject.SetActive(false);
              actionButtonText.text = "Fire";
              AddAccelerometerToMessage();
            }
            else if (role.Equals(cpt))
            {
              actualActionObject.SetActive(false);
              AddGyroToMessage();
            }
          }
          else if (currentLevel == 4)
          { 
            playConstantGlitchEffect(0.1f, 0.517f, 0.234f, 0.661f, 0.492f);

            if (inCave)
            {
              actualActionObject.SetActive(false);
              AddGyroToMessage();
            }
            else if (role.Equals(oc))
            {
              actualActionObject.SetActive(true);
              actionButtonText.text = "Reload";
              AddAccelerometerToMessage();
            }
            else if (role.Equals(wo))
            {
              actualActionObject.SetActive(true);
              actionButtonText.text = "Fire";
              AddAccelerometerToMessage();
            }
            else if (role.Equals(cpt))
            {
              actualActionObject.SetActive(false);
              AddGyroToMessage();
            }
          }
          else if (currentLevel == 2 || currentLevel == 5)
          {
            if (currentLevel == 2)
              playConstantGlitchEffect(0.01f, 0.089f, 0.059f, 0.094f, 0.240f);
            else if (currentLevel == 5)
              playConstantGlitchEffect(0.3f, 0.517f, 0.234f, 0.661f, 0.492f);

            if (inCave)
            {
              actualActionObject.SetActive(false);
              AddGyroToMessage();
            }
            else if (role.Equals(oc))
            {
              actualActionObject.SetActive(false);
              AddGyroToMessage();
            }
            else if (role.Equals(wo))
            {
              actualActionObject.SetActive(true);
              actionButtonText.text = "Reload";
              AddAccelerometerToMessage();
            }
            else if (role.Equals(cpt))
            {
              actualActionObject.SetActive(true);
              actionButtonText.text = "Fire";
              AddAccelerometerToMessage();
            }
          }
          else if (currentLevel == 3)
          {
            playConstantGlitchEffect(0.05f, 0.089f, 0.059f, 0.094f, 0.240f);
          
            if (inCave)
            {
              actualActionObject.SetActive(false);
              AddGyroToMessage();
            }
            else if (role.Equals(oc))
            {
              actualActionObject.SetActive(true);
              actionButtonText.text = "Fire";
              AddAccelerometerToMessage();
            }
            else if (role.Equals(wo))
            {
              actualActionObject.SetActive(false);
              AddGyroToMessage();
            }
            else if (role.Equals(cpt))
            {
              actualActionObject.SetActive(true);
              actionButtonText.text = "Reload";
              AddAccelerometerToMessage();
            }
          }

          if (actionCoolDown)
          {
            action.enabled = false;
            //action.interactable = false;

            if (Time.time - timeOnActionClick > period)
            {
              actionCoolDown = false;
              actualActionObject.GetComponent<Image>().sprite = greenSprite;
            }
            else
              actualActionObject.GetComponent<Image>().sprite = redSprite;

            if (Time.time - timeOnActionClick < period / 2f)
            {
              //Debug.Log("sending message button pressed");
              message += "{B(1)}";
            }
          }
          else
          {
            action.enabled = true;
            //action.interactable = true;
          }

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
        actualActionObject.SetActive(false);
    }
  }

  private void Send(string message, bool printFlag = true)
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

    if (!printFlag)
      return;

    if (printMessageOut)
      Debug.Log(message);
  }
}
