using UnityEngine;

public class ConnectionData : MonoBehaviour
{
  [HideInInspector]
  public string selectedIP;
  [HideInInspector]
  public int selectedPort;

  public int portOut;
  public int portIn;
  public string domeIP;
  public string weIP;
  public string smIP;


  void Start()
  {
    // set default values
    domeIP = "192.168.17.218";
    weIP = "192.168.2.104";
    smIP = "192.168.178.37";
    selectedPort = portOut;
  }
}
