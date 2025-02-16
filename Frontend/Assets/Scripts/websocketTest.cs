using UnityEngine;
using NativeWebSocket;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace DroneTelemetry
{
    [Serializable]
    public class DroneData
    {
        public string drone_id;
        public float timestamp;
        public Position position;
        public Attitude attitude;
        public Velocity velocity;
        public Gyro gyro;
        public Inputs inputs;
        public Battery battery;
        public List<float> motor_rpms;
    }

    [Serializable]
    public class Position { public float x, y, z; }
    [Serializable]
    public class Attitude { public float x, y, z, w; }
    [Serializable]
    public class Velocity { public float x, y, z; }
    [Serializable]
    public class Gyro { public float pitch, roll, yaw; }
    [Serializable]
    public class Inputs { public float throttle, yaw, pitch, roll; }
    [Serializable]
    public class Battery { public float percentage, voltage; }

    public class WebSocketClient : MonoBehaviour
    {
        private WebSocket websocket;

        async void Start()
        {
            websocket = new WebSocket("ws://34.68.252.128:8765/60579 ");
            // help me!
            websocket.OnOpen += () =>
            {
                Debug.Log("WebSocket Connected");
            };

            websocket.OnMessage += (bytes) =>
            {
                string message = System.Text.Encoding.UTF8.GetString(bytes);
                Debug.Log($"Received: {message}");

                try
                {
                    DroneData droneData = JsonConvert.DeserializeObject<DroneData>(message);
                    Debug.Log($"Drone {droneData.drone_id} Position: {droneData.position.x}, {droneData.position.y}, {droneData.position.z}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing drone data: {e.Message}");
                }
            };

            websocket.OnError += (e) =>
            {
                Debug.LogError($"WebSocket Error: {e}");
            };

            websocket.OnClose += (e) =>
            {
                Debug.Log("WebSocket Closed");
            };

            await websocket.Connect();
        }

        async void Update()
        {
            if (websocket.State == WebSocketState.Open)
            {
                await websocket.SendText("ping");
            }
        }

        async void OnApplicationQuit()
        {
            await websocket.Close();
        }
    }
}
