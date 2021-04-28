using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;

[Serializable]
public class Truck
{
	public WheelCollider leftWheel;
	public GameObject leftWheelMesh;
	public WheelCollider rightWheel;
	public GameObject rightWheelMesh;
	public bool motor;
	public bool steering;
	public bool reverseTurn;
}

public class PlayerControls : MonoBehaviour
{
	private Rigidbody rb;
	private float mass;
	private DateTime flipStartTime;
	private ParticleSystem[] particleSystems;
	private bool tapRight;
	private bool tapLeft;
	private Vector3 initialPosition;
	private Quaternion initialRotation;
	private DeviceType DeviceType;

	[Range(0, 1000)]
	public int FlipTimeInMs;
	public float maxMotorTorque;
	public float maxSteeringAngle;
	public List<Truck> truck_Infos;

	public Vector3 CenterOfMass;
	public float radius = 5f;
	[Range(0, 50)]
	public int Smokiness = 3;
	[Range(0.01f, 0.5f)]
	public float lightSwitchSpeed = 0.1f;

	private void Start()
	{
		DeviceType = SystemInfo.deviceType;
		rb = GetComponent<Rigidbody>();
		rb.centerOfMass = CenterOfMass;
		mass = rb.mass;
		flipStartTime = DateTime.Now;
		particleSystems = GetComponentsInChildren<ParticleSystem>();
		initialPosition = transform.position;
		initialRotation = transform.rotation;
	}

	public void VisualizeWheel(Truck wheelPair)
	{
		Quaternion rot;
		Vector3 pos;
		wheelPair.leftWheel.GetWorldPose(out pos, out rot);
		wheelPair.leftWheelMesh.transform.position = pos;
		wheelPair.leftWheelMesh.transform.rotation = rot;

		wheelPair.rightWheel.GetWorldPose(out pos, out rot);

		wheelPair.rightWheelMesh.transform.position = pos;
		wheelPair.rightWheelMesh.transform.rotation = FlipHorizontally(rot);
	}

	public void Update()
	{
		float motor = 0;
		float steering = 0;
		float brakeTorque = 0;

		#region Mobile controls

		if (DeviceType == DeviceType.Handheld)
		{
			tapLeft = tapRight = false;

			if (Input.touches.Length != 0)
            {
                // "touches[0]" - first finger to touch
                CheckTouchSide(Input.touches[0]);

                if (Input.touches.Length > 1)
				{
					// "touches[1]" - second finger to touch
					CheckTouchSide(Input.touches[1]);
				}
            }

			// Right + Left touch = brake
			if (tapLeft && tapRight)
				brakeTorque = 1;
			// Left touch only = reverse
			else if (tapLeft && !tapRight)
				motor = -maxMotorTorque;
			// Right touch only = forward
			else if (tapRight && !tapLeft)
				motor = maxMotorTorque;

			// Steering is limited between +-maxSteeringAngle
			steering = Math.Min(maxSteeringAngle, Math.Max(-maxSteeringAngle, MobileInput.AccelerometerTilt / 1.5f));
		}

		#endregion

		#region Desktop controls
		// WASD & Gamepad

		else if (DeviceType == DeviceType.Desktop)
		{
			motor = maxMotorTorque * Input.GetAxis("Vertical");
			steering = maxSteeringAngle * Input.GetAxis("Horizontal");
			brakeTorque = Mathf.Abs(Input.GetAxis("Jump"));
		}

		#endregion

		if (brakeTorque > 0.001)
		{
			brakeTorque = maxMotorTorque;
			motor = 0;
		}
		else
		{
			brakeTorque = 0;
		}

		foreach (Truck truckInfo in truck_Infos)
		{
			if (truckInfo.steering == true)
			{
				truckInfo.leftWheel.steerAngle = truckInfo.rightWheel.steerAngle = ((truckInfo.reverseTurn) ? -1 : 1) * steering;
			}

			if (truckInfo.motor == true)
			{
				truckInfo.leftWheel.motorTorque = motor;
				truckInfo.rightWheel.motorTorque = motor;
			}

			truckInfo.leftWheel.brakeTorque = brakeTorque;
			truckInfo.rightWheel.brakeTorque = brakeTorque;

			VisualizeWheel(truckInfo);
		}

        #region Visual effects of car
		
  //      // pedal to the metal
  //      if (Input.GetAxis("Vertical") > 0)
		//{
		//	SetTaillightColor(Color.white);
		//	SetMotorSmokeEmission(Input.GetAxis("Vertical") * 30 * Smokiness);
		//}
		//// braking
		//else if (Input.GetAxis("Vertical") < 0 || brakeTorque > 0)
		//{
		//	SetTaillightColor(Color.red);
		//	SetMotorSmokeEmission(2 * Smokiness);
		//}
		//// idle state
		//else
		//{
		//	SetTaillightColor(Color.white);
		//	SetMotorSmokeEmission(10 * Smokiness);
		//}
		
		#endregion

		if (Input.GetKey(KeyCode.R))
		{
			Flip();
		}
	}

    private void CheckTouchSide(Touch touch)
    {
        if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Stationary)
        {
            if (touch.position.x < Screen.width / 2)
            {
                tapLeft = true;
            }
            else if (touch.position.x > Screen.width / 2)
            {
                tapRight = true;
            }
        }
    }

    public void Restart()
	{
		transform.position = initialPosition;
		transform.rotation = initialRotation;
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
	}

	private void SetTaillightColor(Color color)
	{
        //Material myMaterial = (Material)AssetDatabase.LoadAssetAtPath("Assets/Materials/Brakelight.mat", typeof(Material));

        //Color currentAlbedo = myMaterial.GetColor("_Color");
        //Color currentEmission = myMaterial.GetColor("_EmissionColor");

        //if (!color.Equals(currentAlbedo))
        //{
        //    myMaterial.SetColor("_Color", Color.Lerp(currentAlbedo, color, lightSwitchSpeed));
        //    myMaterial.SetColor("_EmissionColor", Color.Lerp(currentEmission, color, lightSwitchSpeed));
        //}
    }


	/// <summary>
	/// Flips the given quaternion parameter horizontally (Z rotation + 180°)
	/// </summary>
	/// <param name="rot">rotation Quaternion to be flipped</param>
	/// <returns>Flipped rotation Quaternion</returns>
	private Quaternion FlipHorizontally(Quaternion rot)
	{
		Vector3 rotEuler = rot.eulerAngles;
		rotEuler = new Vector3(rotEuler.x, rotEuler.y, rotEuler.z + 180);

		return Quaternion.Euler(rotEuler);
	}

	private void Flip()
	{
		// the player is not flipping and the flip time passed
		if (DateTime.Compare(flipStartTime.AddMilliseconds(FlipTimeInMs), DateTime.Now) < 0)
		{
			flipStartTime = DateTime.Now;
			rb.AddForce(new Vector3(0, 5 * mass, 0), ForceMode.Impulse);
			rb.AddRelativeTorque(new Vector3(0, 0, 3 * mass), ForceMode.Impulse);

			// Play the explosion particles
			foreach (var ps in particleSystems)
			{
				if (ps.name == "Light" || ps.name == "Spark" || ps.name == "Smoke")
				{
					ps.Play();
				}
			}
		}
	}


	private void SetMotorSmokeEmission(float val)
	{
		foreach (var ps in particleSystems)
		{
			if (ps.name == "MotorSmoke")
			{
				var e = ps.emission;
				e.enabled = true;
				e.rateOverTime = val;
			}
		}
	}

	/// Gizmo creation for visualizing the center of mass
	//void OnDrawGizmos()
	//{
	//    Gizmos.color = Color.red;
	//    Gizmos.DrawSphere(transform.position + transform.rotation * rb.centerOfMass, radius);
	//}
}