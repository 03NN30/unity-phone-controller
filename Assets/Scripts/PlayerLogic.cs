using UnityEngine;
using UnityEngine.UI;

public class PlayerLogic : Layer
{
  #region GameObjects
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
  #endregion
  #region UI
  [SerializeField]
  Sprite redSprite;
  [SerializeField]
  Sprite greenSprite;

  [SerializeField]
  Button action;
  [SerializeField]
  Button disconnectButton;

  // calibration
  [SerializeField]
  Button initCalibrationButton;

  [SerializeField]
  Text phoneStraightInstructions;
  [SerializeField]
  Button confirmPhoneStraightButton;

  [SerializeField]
  Text flashlightStraightInstructions;
  [SerializeField]
  Button confirmFlashlightStraightButton;
  #endregion

  private void AddRoleToMessage()
  {
    switch (Layer.role)
    {
      case RoleType.OppsCommander:
        Layer.udpClient.Message = "{R(" + RoleType.OppsCommander.ToString() + ")}";
        break;

      case RoleType.WeaponsOfficer:
        Layer.udpClient.Message = "{R(" + RoleType.WeaponsOfficer.ToString() + ")}";
        break;

      case RoleType.Captain:
        Layer.udpClient.Message = "{R(" + RoleType.Captain.ToString() + ")}";
        break;
    }
  }

  private void AddGyroToMessage()
  {
    if (Layer.gyroscope == null)
      return;

    if (Layer.gyroscope.enabled)
    {
      string x = Layer.gyroscope.attitude.x.ToString("G9");
      string y = Layer.gyroscope.attitude.y.ToString("G9");
      string z = Layer.gyroscope.attitude.z.ToString("G9");
      string w = Layer.gyroscope.attitude.w.ToString("G9");

      Layer.udpClient.Message += "{G(" + x + ", " + y + ", " + z + ", " + w + ")}";
    }
  }

  private void AddAccelerometerToMessage()
  {
    if (Layer.accelerometer)
    {
      Vector3 accleration = Input.acceleration;
      Layer.udpClient.Message += "{A" + accleration.ToString("G9") + "}";
    }
  }

  private void Start()
  {
    Hide();
  }

  private void Update()
  {
    if (active)
    {
      AddRoleToMessage();
      AddAccelerometerToMessage();
      AddGyroToMessage();

      Layer.udpClient.Send();
    }
  }

  // gyroscope
  private Quaternion gyro_rotation;
  private Quaternion prev_gyro_rotation = new Quaternion();

  bool actionButtonPressed;
  bool actionCoolDown = false;

  
  private int currentLevel = 1;
  private bool inCave = false;
  private int prevLevel = 1;
  private bool playGlitch = false;
  [SerializeField]
  private float glitchLength = 3.0f;
  private float timeAtLevelChange = 0f;
  float period = 5f;
  float timeOnActionClick = 0f;
  float timeToWaitForCalibrationStep = 2f;
  bool confirmPhoneStraight = false;
  float timeOnConfirmPhoneStraight = 0f;
  bool confirmFlashLightStraight = false;
  float timeOnConfirmFlashlightStraight = 0f;
  bool pressedDisconnect = false;
  float timeOnDisconnect = 0f;
  float timeToSendDisconnectMessage = 2f;
  bool sendVerificationMessage = true;
  float timeTracker = 0f;
  float timeSinceVerificationInit = 0f;
  float timeToSendVerification = 2.5f;

  /*
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
          PlayerSelection.udpClient.Send("{" + ConnectionData.localIP + "}" + "{R(" + correctRoleString + ")}{D(" + correctRoleString + ")}");
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
      if (PlayerSelection.udpClient == null)
        Debug.LogError("Next you say \"it's null\"");

      // send message every 5 seconds for 2.5 seconds to check that this client is still online
      if (Time.time > timeTracker)
      {
        timeTracker += 5f;
        sendVerificationMessage = true;
        timeSinceVerificationInit = Time.time;
      }

      if (sendVerificationMessage)
      {
        PlayerSelection.udpClient.Send("{" + ConnectionData.localIP + "}" + "{R(" + correctRoleString + ")}{P(" + correctRoleString + ")}");

        if (Time.time - timeSinceVerificationInit > timeToSendVerification)
        {
          sendVerificationMessage = false;
        }

        presentText.text = "P(1) ";
      }
      else
      {
        presentText.text = "P(0) ";
      }

      if (PlayerSelection.udpClient.ReceivedMessage != null || PlayerSelection.udpClient.ReceivedMessage != "")
      {
        int foundLevel = PlayerSelection.udpClient.ReceivedMessage.IndexOf("{L(");
        if (foundLevel != -1)
          currentLevel = Int32.Parse(PlayerSelection.udpClient.ReceivedMessage.Substring(foundLevel + 3, 1));

        int foundInCave = PlayerSelection.udpClient.ReceivedMessage.IndexOf("{C(");
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

          if (currentLevel == 1)
          {
            playConstantGlitchEffect(0f, 0f, 0f, 0f, 0f);

            if (!inCave && !role.Equals(cpt))
            {
              initCalibrationButton.gameObject.SetActive(false);
            }

            if (inCave)
            {
              actualActionObject.SetActive(false);
              AddGyroToMessage();
              initCalibrationButton.gameObject.SetActive(true);
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

            if (!inCave && !role.Equals(cpt))
            {
              initCalibrationButton.gameObject.SetActive(false);
            }

            if (inCave)
            {
              actualActionObject.SetActive(false);
              AddGyroToMessage();
              initCalibrationButton.gameObject.SetActive(true);
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

            if (!inCave && !role.Equals(cpt))
            {
              initCalibrationButton.gameObject.SetActive(false);
            }

            if (inCave)
            {
              actualActionObject.SetActive(false);
              AddGyroToMessage();
              initCalibrationButton.gameObject.SetActive(true);
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

            if (!inCave && !role.Equals(cpt))
            {
              initCalibrationButton.gameObject.SetActive(false);
            }

            if (inCave)
            {
              actualActionObject.SetActive(false);
              AddGyroToMessage();
              initCalibrationButton.gameObject.SetActive(true);
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

            if (Time.time - timeOnActionClick > period)
            {
              actionCoolDown = false;
              actualActionObject.GetComponent<Image>().sprite = greenSprite;
            }
            else
              actualActionObject.GetComponent<Image>().sprite = redSprite;

            if (Time.time - timeOnActionClick < period / 2f)
            {
              message += "{B(1)}";
            }
          }
          else
          {
            action.enabled = true;
          }

          if (actionButtonPressed)
          {
            actionButtonPressed = false;
          }

          // only send message if not empty
          if (message.Length > 0)
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
            if (message.IndexOf("{G(") != -1)
              gyroText.text = "G(1) ";
            else
              gyroText.text = "G(0) ";

            if (message.IndexOf("{A(") != -1)
              accelText.text = "A(1) ";
            else
              accelText.text = "A(0) ";

            if (message.IndexOf("{B(") != -1)
              buttonText.text = "B(1) ";
            else
              buttonText.text = "B(0) ";

            if (message.IndexOf("{D(") != -1)
              disconnectText.text = "D(1) ";
            else
              disconnectText.text = "D(0) ";

            if (message.IndexOf("{R(?)") != -1)
              roleAskText.text = "R?(1) ";
            else
              roleAskText.text = "R?(0) ";

            int rolePos = message.IndexOf("{R(");
            if (rolePos != -1)
            {
              string roleData = message.Substring(rolePos + 3);
              roleData = roleData.Substring(0, roleData.IndexOf(")") - 1);

              roleSendText.text = "R(" + roleData;
            }
            else
              roleSendText.text = "R(0)";

            // send final message
            PlayerSelection.udpClient.Send("{" + ConnectionData.localIP + "}" + message);
          }
        }
      }
      else
        actualActionObject.SetActive(false);
    }
  }

  */


  ///////////////////////////////////////////////////////////////////////////////////////////////

  /*
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

  public override void Hide()
  {
    active = false;
    GetComponent<CanvasGroup>().alpha = 0f;
    GetComponent<CanvasGroup>().blocksRaycasts = false;

    stopGlitchEffect();
  }

  void Start()
  {
    Hide();

    setPhoneStraightUI(false);
    setFlashlightStraightUI(false);

    actualActionObject.GetComponent<Image>().sprite = greenSprite;
  }

  void Calibration()
  {
    if (confirmPhoneStraight)
    {
      if (Time.time - timeOnConfirmPhoneStraight > timeToWaitForCalibrationStep)
      {
        Debug.Log("Calibration: Phone straight off");
        confirmPhoneStraight = false;
      }
      string message_t = "{C(P)}";
      PlayerSelection.udpClient.Send("{" + ConnectionData.localIP + "}" + message_t + "{R(3)}");
    }
    else if (confirmFlashLightStraight)
    {
      if (Time.time - timeOnConfirmFlashlightStraight > timeToWaitForCalibrationStep)
      {
        Debug.Log("Calibration: Flashlight straight off");
        confirmFlashLightStraight = false;
      }
      string message_t = "{C(F)}";
      PlayerSelection.udpClient.Send("{" + ConnectionData.localIP + "}" + message_t + "{R(3)}");
    }
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
  */
}
