using UnityEngine;
using UnityEngine.UI;

public enum Action { Reload, Fire }

public class PlayerLogic : Layer
{
  #region GameObjects
  [SerializeField]
  private GameObject calibration;
  [SerializeField]
  private GameObject cam;
  [SerializeField]
  private GameObject startScreen;
  [SerializeField]
  private GameObject action;
  #endregion

  #region UI
  [SerializeField]
  private Sprite redSprite;
  [SerializeField]
  private Sprite greenSprite;

  [SerializeField]
  private Button actionButton;
  [SerializeField]
  private Button disconnectButton;

  // calibration
  [SerializeField]
  private Button startCalibrationButton;
  [SerializeField]
  private Text phoneStraightInstructions;
  [SerializeField]
  private Button confirmPhoneStraightButton;

  [SerializeField]
  private Text flashlightStraightInstructions;
  [SerializeField]
  private Button confirmFlashlightStraightButton;
  #endregion

  [SerializeField]
  private float coolDownLength = 5f;
  private float timeOnActionPressed = 0f;
  private bool coolDown = false;
  private bool captureTimeOnLevelChange = true;

  #region Buttons
  private void OnEnable()
  {
    disconnectButton.onClick.AddListener(DisconnectPressed);
    startCalibrationButton.onClick.AddListener(StartCalibrationPressed);
    confirmPhoneStraightButton.onClick.AddListener(ConfirmPhoneStraightPressed);
    confirmFlashlightStraightButton.onClick.AddListener(ConfirmFlashlightStraightPressed);
    actionButton.onClick.AddListener(ActionPressed);
  }

  private void ActionPressed()
  {
    if (coolDown)
      return;

    Layer.tcpClient.Send(PackageType.Action, Layer.Role.ToString());
    timeOnActionPressed = Time.time;
    actionButton.GetComponent<Image>().sprite = redSprite;
    coolDown = true;
  }

  private void DisconnectPressed()
  {
    Layer.tcpClient.Send(PackageType.Disconnected, Layer.Role.ToString());
    Hide();
    startScreen.GetComponent<StartScreen>().Show();
  }

  private void StartCalibrationPressed()
  {
    confirmPhoneStraightButton.gameObject.SetActive(!confirmPhoneStraightButton.gameObject.active);
    phoneStraightInstructions.enabled = !phoneStraightInstructions.enabled;

    SetFlashlightStraightUI(false);
  }
  #endregion

  #region Calibration
  private void SetPhoneStraightUI(bool visible)
  {
    confirmPhoneStraightButton.gameObject.SetActive(visible);
    phoneStraightInstructions.enabled = visible;
  }

  private void SetFlashlightStraightUI(bool visible)
  {
    confirmFlashlightStraightButton.gameObject.SetActive(visible);
    flashlightStraightInstructions.enabled = visible;
  }

  private void ConfirmPhoneStraightPressed()
  {
    SetPhoneStraightUI(false);
    SetFlashlightStraightUI(true);

    Layer.tcpClient.Send(PackageType.Calibrate1, Layer.Role.ToString());
  }

  private void ConfirmFlashlightStraightPressed()
  {
    SetPhoneStraightUI(false);
    SetFlashlightStraightUI(false);

    Layer.tcpClient.Send(PackageType.Calibrate2, Layer.Role.ToString());
     
    startCalibrationButton.image.color = Color.green;
  }
  #endregion

  #region Messages
  private void AddRoleToMessage()
  {
    switch (Layer.Role)
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
    calibration.SetActive(true);
    action.SetActive(false);

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

  private void AddAccelerometerToMessage(Action type)
  {
    if (Layer.Level == 1)
      action.SetActive(false);
    else
      action.SetActive(true);
    
    calibration.SetActive(false);

    switch (type)
    {
      case Action.Reload:
        actionButton.transform.GetChild(0).GetComponent<Text>().text = "Reload";
        break;

      case Action.Fire:
        actionButton.transform.GetChild(0).GetComponent<Text>().text = "Fire";
        break;
    }

    if (Layer.accelerometer)
    {
      Vector3 accleration = Input.acceleration;
      Layer.udpClient.Message += "{A" + accleration.ToString("G9") + "}";
    }
  }
  #endregion

  #region Glitch
  private void StopGlitchEffect()
  {
    cam.GetComponent<Kino.DigitalGlitch>().enabled = false;
    cam.GetComponent<Kino.AnalogGlitch>().enabled = false;
  }
  
  private void PlayGlitchEffect(float intensity, float scanLineJitter, float verticalJump, float horizontalShake, float colorDrift)
  {
    var digital = cam.GetComponent<Kino.DigitalGlitch>();
    digital.enabled = true;
    digital.intensity = intensity;

    var analog = cam.GetComponent<Kino.AnalogGlitch>();
    analog.enabled = true;
    analog.scanLineJitter = scanLineJitter;
    analog.verticalJump = verticalJump;
    analog.horizontalShake = horizontalShake;
    analog.colorDrift = colorDrift;
  }
  #endregion

  public override void Hide()
  {
    base.Hide();
    StopGlitchEffect();
  }

  private void Start()
  {
    Hide();
    SetPhoneStraightUI(false);
    SetFlashlightStraightUI(false);

    actionButton.GetComponent<Image>().sprite = greenSprite;
    Layer.Level = 1;
  }

  private void Update()
  {
    if (active)
    {
      if (Time.time - timeOnActionPressed > coolDownLength)
      {
        coolDown = false;
        actionButton.GetComponent<Image>().sprite = greenSprite;
      }

      // make sure UI disappears temporarily on level changed event
      if (!Layer.LevelChanged)
      {
        if (Layer.Role == RoleType.OppsCommander)
        {
          switch (Layer.Level)
          {
            case 0:
              AddGyroToMessage();
              break;

            case 1:
              AddAccelerometerToMessage(Action.Reload);
              break;

            case 2:
              AddAccelerometerToMessage(Action.Fire);
              break;

            case 3:
              AddGyroToMessage();
              break;

            case 4:
              AddAccelerometerToMessage(Action.Reload);
              break;

            case 5:
              AddAccelerometerToMessage(Action.Fire);
              break;
          }
        }
        else if (Layer.Role == RoleType.WeaponsOfficer)
        {
          switch (Layer.Level)
          {
            case 0:
              AddGyroToMessage();
              break;

            case 1:
              AddAccelerometerToMessage(Action.Fire);
              break;

            case 2:
              AddGyroToMessage();
              break;

            case 3:
              AddAccelerometerToMessage(Action.Reload);
              break;

            case 4:
              AddAccelerometerToMessage(Action.Fire);
              break;

            case 5:
              AddGyroToMessage();
              break;
          }
        }
        else if (Layer.Role == RoleType.Captain)
        {
          switch (Layer.Level)
          {
            case 0:
              AddGyroToMessage();
              break;

            case 1:
              AddGyroToMessage();
              break;

            case 2:
              AddAccelerometerToMessage(Action.Reload);
              break;

            case 3:
              AddAccelerometerToMessage(Action.Fire);
              break;

            case 4:
              AddGyroToMessage();
              break;

            case 5:
              AddAccelerometerToMessage(Action.Reload);
              break;
          }
        }

        AddRoleToMessage();
        Layer.udpClient.Send();
      }
      else
      {
        calibration.SetActive(false);
        action.SetActive(false);
      }

      // play glitch effect on level change
      if (Layer.LevelChanged)
      {
        if (captureTimeOnLevelChange)
        {
          Layer.TimeOnLevelChanged = Time.time;
          captureTimeOnLevelChange = false;
        }

        if (Time.time - Layer.TimeOnLevelChanged > 2.5f)
        {
          Layer.LevelChanged = false;
          captureTimeOnLevelChange = true;
        }

        PlayGlitchEffect(0.637f, 0.545f, 0.216f, 0f, 0.521f);
        return;
      }

      // play constant glitch effect
      switch (Layer.Level)
      {
        case 1:
          PlayGlitchEffect(0f, 0f, 0f, 0f, 0f);
          break;

        case 2:
          PlayGlitchEffect(0.01f, 0.1f, 0f, 0f, 0f);
          break;

        case 3:
          PlayGlitchEffect(0.02f, 0.2f, 0.011f, 0.011f, 0.011f);
          break;

        case 4:
          PlayGlitchEffect(0.03f, 0.3f, 0.022f, 0.022f, 0.022f);
          break;

        case 5:
          PlayGlitchEffect(0.04f, 0.4f, 0.033f, 0.033f, 0.033f);
          break;
      }
    }
  }
}
