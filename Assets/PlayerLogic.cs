using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine.UI;

public class PlayerLogic : MonoBehaviour
{
  private IPEndPoint remoteEndPoint;
  private UdpClient client;

  [HideInInspector]
  public bool valid_input;
  private bool valid_connection;

  private string message;

  [SerializeField]
  GameObject connectionData;

  [SerializeField]
  Text errorMessage;

  // use own local IP address to identify player
  private string localIP;

  // accelerometer
  private bool accelerometer_enabled;
  private Vector3 prev_accelerometer = new Vector3();

  public void hide()
  {
    GetComponent<CanvasGroup>().alpha = 0f;
    GetComponent<CanvasGroup>().blocksRaycasts = false;
  }

  public void show()
  {
    GetComponent<CanvasGroup>().alpha = 1f;
    GetComponent<CanvasGroup>().blocksRaycasts = true;
  }

  void Start()
  {
    hide();

    valid_connection = false;
    valid_input = false;

    accelerometer_enabled = false;

    // accelerometer
    if (SystemInfo.supportsAccelerometer)
    {
      accelerometer_enabled = true;
    }
    else
    {
      Debug.LogError("Your device doesn't have an accelerometer");
      errorMessage.text = "This device does not have an accelerometer";
    }
  }

  void Update()
  {
    // delete content of last message
    message = "";

    if (valid_input && !valid_connection)
    {
      valid_connection = true;

      Debug.Log(connectionData.GetComponent<ConnectionData>().selectedIP);
      Debug.Log(connectionData.GetComponent<ConnectionData>().selectedPort);

      remoteEndPoint = new IPEndPoint(
        IPAddress.Parse(connectionData.GetComponent<ConnectionData>().selectedIP), 
        Int32.Parse(connectionData.GetComponent<ConnectionData>().selectedPort)
      );

      client = new UdpClient();

      localIP = GetLocalIPAddress().ToString();

      Debug.Log("Connection Established");
    }

    // send data for horizontal and vertical movement using the accelerometer
    if (accelerometer_enabled)
    {
      Vector3 temp = Input.acceleration;

      if (prev_accelerometer != temp)
        message += "{A" + temp.ToString() + "}";
    }

    // only send message if not empty
    if (message.Length > 0 && valid_connection)
    {
      Send("{" + localIP + "}" + message);
    }
  }

  private void Send(string message)
  {
    try
    {
      byte[] data = Encoding.UTF8.GetBytes(message);
      client.Send(data, data.Length, remoteEndPoint);
    }
    catch (Exception e)
    {
      print(e.ToString());
    }
  }

  public static string GetLocalIPAddress()
  {
    var host = Dns.GetHostEntry(Dns.GetHostName());
    foreach (var ip in host.AddressList)
    {
      if (ip.AddressFamily == AddressFamily.InterNetwork)
      {
        return ip.ToString();
      }
    }
    throw new Exception("No network adapters with an IPv4 address in the system!");
  }
}
