using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class Dot_Truck : System.Object
{
	public WheelCollider leftWheel;
	public GameObject leftWheelMesh;
	public WheelCollider rightWheel;
	public GameObject rightWheelMesh;
	public bool motor;
	public bool steering;
	public bool reverseTurn; 
}

public class Dot_Truck_Controller : MonoBehaviour {
	private Rigidbody rb;
	private float mass;
	private DateTime flipStartTime;
	ParticleSystem[] particleSystems;


	[Range(0,1000)]
	public int FlipTimeInMs;
	public float maxMotorTorque;
	public float maxSteeringAngle;
	public List<Dot_Truck> truck_Infos;

	public Vector3 CenterOfMass;
	public float radius = 5f;
	[Range(0,50)]
	public int Smokiness = 3;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		mass = rb.mass;
		flipStartTime = DateTime.Now;
		particleSystems = GetComponentsInChildren<ParticleSystem>();
	}

    public void VisualizeWheel(Dot_Truck wheelPair)
	{
		Quaternion rot;
		Vector3 pos;
		wheelPair.leftWheel.GetWorldPose ( out pos, out rot);
		wheelPair.leftWheelMesh.transform.position = pos;
		wheelPair.leftWheelMesh.transform.rotation = rot;
		wheelPair.rightWheel.GetWorldPose ( out pos, out rot);
		wheelPair.rightWheelMesh.transform.position = pos;
		wheelPair.rightWheelMesh.transform.rotation = rot;
	}

	public void Update()
	{
		float motor = maxMotorTorque * Input.GetAxis("Vertical");
		float steering = maxSteeringAngle * Input.GetAxis("Horizontal");
		float brakeTorque = Mathf.Abs(Input.GetAxis("Jump"));
		if (brakeTorque > 0.001) {
			brakeTorque = maxMotorTorque;
			motor = 0;
		} else {
			brakeTorque = 0;
		}

		foreach (Dot_Truck truck_Info in truck_Infos)
		{
			if (truck_Info.steering == true) {
				truck_Info.leftWheel.steerAngle = truck_Info.rightWheel.steerAngle = ((truck_Info.reverseTurn)?-1:1)*steering;
			}

			if (truck_Info.motor == true)
			{
				truck_Info.leftWheel.motorTorque = motor;
				truck_Info.rightWheel.motorTorque = motor;
			}

			truck_Info.leftWheel.brakeTorque = brakeTorque;
			truck_Info.rightWheel.brakeTorque = brakeTorque;

			VisualizeWheel(truck_Info);

			if (Input.GetAxis("Vertical") > 0)
            {
                SetMotorSmokeEmission(Input.GetAxis("Vertical") * 30 * Smokiness);
            }
            else
            {
				SetMotorSmokeEmission(10 * Smokiness);
			}
		}

		if (Input.GetKey(KeyCode.R))
		{
			// the player is not flipping and the flip time passed
			if (DateTime.Compare(flipStartTime.AddMilliseconds(FlipTimeInMs), DateTime.Now) < 0)
			{
				flipStartTime = DateTime.Now;
				rb.AddForce(new Vector3(0, 5 * mass, 0), ForceMode.Impulse);
				rb.AddRelativeTorque(new Vector3(0, 0, 3 * mass), ForceMode.Impulse);

				foreach (var ps in particleSystems)
				{
					if (ps.name == "Light" || ps.name == "Spark" || ps.name == "Smoke")
					{
						ps.Play();
					}
				}
			}
		}
		rb.centerOfMass = CenterOfMass;
		//rb.WakeUp();
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

    //void OnDrawGizmos()
    //{
    //	Gizmos.color = Color.red;
    //	Gizmos.DrawSphere(transform.position + transform.rotation * rb.centerOfMass, radius);
    //}


}