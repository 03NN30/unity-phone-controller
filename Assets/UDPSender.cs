using UnityEngine;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class UDPSender : MonoBehaviour
{
  // connection
  private string ip;
  private string port;

  private IPEndPoint remoteEndPoint;
  private UdpClient client;

  private string message;

  // use mac address to identify every player
  private string mac;

  private bool valid_input;
  private bool valid_connection;

  // input from ui
  [SerializeField]
  InputField in_message;
  [SerializeField]
  InputField in_ip;
  [SerializeField]
  InputField in_port;
  [SerializeField]
  Button connect;
  [SerializeField]
  Button send;

  [SerializeField]
  Text out_gyro;
  [SerializeField]
  Text gyro_label;
  [SerializeField]
  Toggle gyro_toggle;
  [SerializeField]
  Image gyro_toggle_image;

  [SerializeField]
  Text accelerometer_label;
  [SerializeField]
  Text out_accelerometer;
  [SerializeField]
  Toggle accelerometer_toggle;
  [SerializeField]
  Image accelerometer_toggle_image;

  [SerializeField]
  Text own_ip_address;
  [SerializeField]
  Text last_message_sent;

  // gyroscope
  private Gyroscope gyro;
  private Quaternion gyro_rotation;
  private Quaternion prev_gyro_rotation = new Quaternion();
  private bool gyro_enabled;

  // accelerometer
  private bool accelerometer_enabled;
  private Vector3 prev_accelerometer = new Vector3();

  void OnEnable()
  {
    connect.onClick.AddListener(ConnectPressed);
    send.onClick.AddListener(SendPressed);
  }

  private void ConnectPressed()
  {
    Match match_ip = Regex.Match(in_ip.text, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
    Match match_port = Regex.Match(in_port.text, @"\b\d{1,6}\b");
    if (match_ip.Success && match_port.Success)
    {
      Debug.Log(in_ip.text);

      valid_input = true;
      ip = in_ip.text;
      port = in_port.text;

      connect.image.color = Color.green;
    }
    else
    {
      connect.image.color = Color.white;
      Debug.LogWarning("Invalid Input");
    }
  }

  private void SendPressed()
  {
    if (valid_connection)
    {
      if (send)
      {
        if (!string.IsNullOrEmpty(in_message.text))
        {
          Send(in_message.text);
          Debug.Log("Send Message: " + in_message.text);
        }
        else
          Debug.Log("Invalid message");
      }
    }
  }

  public void Start()
  {
    own_ip_address.text = GetLocalIPAddress().ToString();

    valid_connection = false;
    valid_input = false;
    gyro_enabled = false;
    accelerometer_enabled = false;

    // on by default
    gyro_toggle.isOn = true;
    accelerometer_toggle.isOn = true;

    // accelerometer
    if (SystemInfo.supportsAccelerometer)
    {
      accelerometer_enabled = true;
    }
    else
    {
      Debug.LogError("Your device doesn't have an accelerometer");

      // hide accelerometer label
      out_accelerometer.text = "not supported by this device";
      accelerometer_toggle.isOn = false;
      accelerometer_toggle_image.color = Color.grey;
    }

    // gyroscope
    if (SystemInfo.supportsGyroscope)
    {
      gyro = Input.gyro;
      gyro.enabled = true;
      gyro_enabled = true;
    }
    else
    {
      Debug.LogError("Your device doesn't have a gyroscope");

      // hide gyroscope label
      out_gyro.text = "not supported by this device";
      gyro_toggle.isOn = false;
      gyro_toggle_image.color = Color.grey;
    }
  }

  public void Update()
  {
    // delete content of last message
    message = "";

    if (valid_input && !valid_connection)
    {
      valid_connection = true;

      remoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), Int32.Parse(port));
      client = new UdpClient();

      mac = GetLocalIPAddress().ToString();

      Debug.Log("Connection Established");
    }

    if (gyro_enabled && gyro_toggle.isOn)
    {
      gyro_rotation = gyro.attitude;

      // only send if field has changed
      if (prev_gyro_rotation != gyro_rotation)
      {
        message += "{G" + gyro_rotation.ToString() + "}";
      }
      prev_gyro_rotation = gyro_rotation;

      // print gyro rotation to screen
      out_gyro.text = gyro_rotation.ToString();
    }

    if (accelerometer_enabled && accelerometer_toggle.isOn)
    {
      Vector3 temp = Input.acceleration;

      if (prev_accelerometer != temp)
        message += "{A" + temp.ToString() + "}";

      out_accelerometer.text = temp.ToString();
    }

    // only send message if not empty
    if (message.Length > 0 && valid_connection)
    {
      Send("{" + mac + "}" + message);

      last_message_sent.text = "{" + mac + "}" + message;

      /* TODO: add adjustable tick rate */
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