using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class Credits : Layer
{
  [SerializeField]
  private float fadeToWhitePeriod = 5f;
  [SerializeField]
  private float rollInPeriod = 30f;

  [SerializeField]
  private GameObject blackScreen = null;
  [SerializeField]
  private GameObject everyText = null;
  [SerializeField]
  private Text deep = null;
  [SerializeField]
  private GameObject credits = null;

  public static bool Trigger { get; set; }

  public static float TimeOnVictory { get; set; }

  private bool firstRun = true;
  private float timeOnRollIn = 0f;


  private void Start()
  {
    Trigger = false;
    Hide();
  }

  private void Update()
  {
    if (Trigger)
    {
      var playerLogic = GameObject.Find("GameScreen").GetComponent<PlayerLogic>();
      playerLogic.Hide();

      Show();

      var particles = GameObject.Find("Particle").GetComponent<VideoPlayer>();
      particles.Stop();

      blackScreen.GetComponent<Image>().enabled = true;
      blackScreen.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);

      TimeOnVictory = Time.time;

      Trigger = false;
    }

    if (!active)
      return;

    if (Time.time - TimeOnVictory < fadeToWhitePeriod)
    {
      float distCovered = (Time.time - TimeOnVictory) * 1f;
      float fractionOfJourney = distCovered / fadeToWhitePeriod;

      var res = Mathf.Lerp(0f, 1f, fractionOfJourney);

      blackScreen.GetComponent<Image>().color = new Color(0.5865966f, 0.735849f, 0.7192654f, res);

      deep.color = new Color(0f, 0f, 0f, res);
    }
    else if (Time.time - TimeOnVictory > fadeToWhitePeriod * 2)
    {
      if (firstRun)
      {
        credits.SetActive(true);
        firstRun = false;
        timeOnRollIn = Time.time;
      }

      float distCovered = (Time.time - timeOnRollIn) * 1f;
      float fractionOfJourney = distCovered / rollInPeriod;
      float moveUp = Mathf.Lerp(0f, 1710f, fractionOfJourney);

      //everyText.transform.position = new Vector3(0f, moveUp, 0f);
      everyText.GetComponent<RectTransform>().localPosition = new Vector3(0f, moveUp, 0f);
    }
  }
}
