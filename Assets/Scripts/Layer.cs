using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Layer : MonoBehaviour
{
  protected static TCPClient tcpClient;
  protected static UDPClient udpClient;

  protected static Gyroscope gyroscope;
  protected static bool accelerometer = false;

  protected bool active = false;
  [HideInInspector]
  public static RoleType Role { get; set; }
  [HideInInspector]
  public static int Level { get; set; }
  [HideInInspector]
  public static bool LevelChanged { get; set; }
  
  public static float TimeOnLevelChanged { get; set; }

  public virtual void Hide()
  {
    active = false;
    GetComponent<CanvasGroup>().alpha = 0f;
    GetComponent<CanvasGroup>().blocksRaycasts = false;
  }

  public virtual void Show()
  {
    active = true;
    GetComponent<CanvasGroup>().alpha = 1f;
    GetComponent<CanvasGroup>().blocksRaycasts = true;
  }
}
