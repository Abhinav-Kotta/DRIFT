using System;
using Unity.VisualScripting;
using UnityEngine;

public class DroneMover : MonoBehaviour
{
    public float angle = 0f;
    [SerializeField] float x;
    [SerializeField] float y;
    [SerializeField] float z;

    [SerializeField] float roll;
    [SerializeField] float pitch;
    [SerializeField] float yaw;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {
        simulateMovement();
        transform.position = getPosition();
        transform.rotation = getRotation();
    }
    public Vector3 getPosition()
    {
        return new Vector3(x, y, z);
    }
    public Quaternion getRotation()
    {
        return Quaternion.Euler(pitch, yaw, roll);
    }

    void simulateMovement()
    {
        x = (float)Math.Cos(angle) * 10f;
        z = (float)Math.Sin(angle) * 10f; 
        roll = (float)Math.Sin(angle) * 25f;
        pitch = (float)Math.Cos(angle) * 25f;


        angle += 2 * Time.deltaTime;
    }
}
