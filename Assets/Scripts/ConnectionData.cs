using UnityEngine;

public class ConnectionData : MonoBehaviour
{
  [HideInInspector]
  public static string selectedIP;

  public const int portOutTCP = 11000;
  public const int portOutUDP = 5555;
  public const int portInUDP = 5556;

  public const string domeIP = "192.168.0.101";
  public const string weIP = "192.168.2.104";
  public const string smIP = "192.168.178.39";

  public static string localIP;

  private void Start()
  {
    localIP = UDPClient.GetLocalIPAddress();
  }
}
