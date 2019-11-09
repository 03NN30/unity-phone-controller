using UnityEngine;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

public class UDPSender : MonoBehaviour
{
  // connection
  private string ip;
  private string port;

  private IPEndPoint remoteEndPoint;
  private UdpClient client;

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

  // gyroscope
  private Gyroscope gyro;
  private Quaternion gyro_rotation;
  private bool gyro_enabled;

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
    valid_connection = false;
    valid_input = false;
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
      Debug.LogError("Your device doesn't have a gyroscope");

      // hide gyroscope label
      gyro_label.enabled = false;
    }
  }

  public void Update()
  {
    if (valid_input && !valid_connection)
    {
      valid_connection = true;

      remoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), Int32.Parse(port));
      client = new UdpClient();

      mac = GetMacAddress().ToString();

      Debug.Log("Connection Established");
    }

    if (gyro_enabled)
    {
      gyro_rotation = gyro.attitude;

      // print gyro rotation to screen
      out_gyro.text = gyro_rotation.ToString();

      /* TODO: add adjustable rate */
      if (valid_connection)
        Send(mac + "_" + gyro_rotation.ToString());
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

  public static PhysicalAddress GetMacAddress()
  {
    foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
    {
      if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet && nic.OperationalStatus == OperationalStatus.Up)
        return nic.GetPhysicalAddress();
    }
    return null;
  }
}