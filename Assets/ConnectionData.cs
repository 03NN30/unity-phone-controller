using UnityEngine;

public class ConnectionData : MonoBehaviour
{
  [HideInInspector]
  public string selectedIP;
  [HideInInspector]
  public string selectedPort;

  public string port;
  public string domeIP;
  public string weIP;
  public string smIP;

  void Start()
  {
    // set default values
    domeIP = "";
    weIP = "";
    smIP = "192.168.178.37";
    selectedPort = port;
  }
}
