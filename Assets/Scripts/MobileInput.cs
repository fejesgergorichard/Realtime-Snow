using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileInput : MonoBehaviour
{
    private static bool gyroInitialized = false;

    public static bool HasGyroscope
    {
        get
        {
            return SystemInfo.supportsGyroscope;
        }
    }

    private static Quaternion _initialGyroRotation;
    public static Quaternion InitialGyroRotation
    {
        get
        {
            if (!gyroInitialized)
            {
                InitGyro();
            }

            return HasGyroscope
                ? _initialGyroRotation
                : new Quaternion();
        }
        private set
        {
            _initialGyroRotation = value;
        }
    }

    public static Quaternion GyroRotation
    {
        get
        {
            if (!gyroInitialized)
            {
                InitGyro();
            }

            return HasGyroscope
                ? ReadGyroRotation()
                : new Quaternion();
        }
    }
    public static float AccelerometerTilt
    {
        get
        {
            if (!gyroInitialized)
            {
                InitGyro();
            }

            return HasGyroscope
                ? ReadTiltAngle()
                : 0.0f;
        }
    }

    private static void InitGyro()
    {
        if (HasGyroscope)
        {
            Input.gyro.enabled = true;                // enable the gyroscope
            Input.gyro.updateInterval = 0.0167f;      // set the update interval to it's highest value (60 Hz)
        }
        gyroInitialized = true;
    }

    private static float ReadTiltAngle()
    {
        var deviceTilt = Input.acceleration;
        return deviceTilt.x * 90;
    }

    private static Quaternion ReadGyroRotation()
    {
        return new Quaternion(0.5f, 0.5f, -0.5f, 0.5f) * Input.gyro.attitude * new Quaternion(0, 0, 1, 0);
    }

    private void Start()
    {
        InitialGyroRotation = GyroRotation;
    }
}
