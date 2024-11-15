using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

public class DroneVisualizer : MonoBehaviour
{
    private GameObject dronePoint;
    private Socket socket;
    private byte[] buffer = new byte[2048];
    private EndPoint remoteEP;
    private Vector3 lastPosition = Vector3.zero;

    private void Start()
    {
        Debug.Log("Starting DroneVisualizer...");
        InitializeVisualization();
        InitializeSocket();
    }

    private void InitializeVisualization()
    {
        dronePoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dronePoint.transform.localScale = Vector3.one * 0.2f;
        dronePoint.GetComponent<Renderer>().material.color = Color.red;
        Debug.Log($"Sphere created at position: {dronePoint.transform.position}");
    }

    private void InitializeSocket()
    {
        try
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(new IPEndPoint(IPAddress.Any, 5555));
            socket.Blocking = false;
            remoteEP = new IPEndPoint(IPAddress.Any, 0);
            Debug.Log("Socket initialized on port 5555");
        }
        catch (Exception e)
        {
            Debug.LogError($"Socket initialization error: {e.Message}");
        }
    }

    private void Update()
    {
        if (socket == null) return;

        try
        {
            int received = 0;
            try
            {
                Debug.Log("in here");
                received = socket.ReceiveFrom(buffer, ref remoteEP);
                Debug.Log($"received data: {received}");
            }
            catch (SocketException e)
            {
                // WSAEWOULDBLOCK = 10035 (Would block) is normal for non-blocking sockets
                if (e.ErrorCode != 10035)
                {
                    Debug.LogError($"Socket error: {e.Message} (Error code: {e.ErrorCode})");
                }
                return;
            }

            if (received > 0)
            {
                string jsonData = Encoding.UTF8.GetString(buffer, 0, received);
                Debug.Log($"Received {received} bytes: {jsonData}");

                var droneData = JsonConvert.DeserializeObject<DroneData>(jsonData);
                if (droneData != null)
                {
                    Vector3 newPosition = new Vector3(
                        droneData.Position.X,
                        -droneData.Position.Z,
                        droneData.Position.Y
                    );

                    Debug.Log($"Moving sphere to {newPosition}");
                    dronePoint.transform.position = newPosition;
                    lastPosition = newPosition;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"General error: {e.Message}");
        }
    }

    private void OnDestroy()
    {
        if (socket != null)
        {
            socket.Close();
            socket = null;
        }
    }
}