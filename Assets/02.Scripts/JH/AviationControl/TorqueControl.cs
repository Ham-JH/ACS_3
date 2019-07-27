﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorqueControl : MonoBehaviour
{
	[Header("Aviation_status")]
	public AviationManager aviationManager;
	[HideInInspector] public AviationManager.AviationStatus status;

	[Header("Force_status")]
	public ThrustControl thrustControl;

	[Header("Fighter")]
	public Rigidbody fighterBody;

	[Header("Force values")]
	public float currForce		= 0f;				//현재 추력값
	public float TorqueResist	= 0f;				//회전항력

	[Header("Torque_values")]
	[HideInInspector] public Quaternion controlAngle;

	[Header("inputCheck")]
	public float pitchVal = 0;
	public float yawVal = 0;
	public float rollVal = 0;

	[Header("Speed_boundary")]
	[HideInInspector] public float _minSB;	//최소 속도경계
	[HideInInspector] public float _norSB;  //평균 속도경계
	[HideInInspector] public float _maxSB;  //최대 속도경계

	// Start is called before the first frame update
	void Start()
    {
		//속도경계 설정
		_minSB = aviationManager.minimumSpeedBoundary;
		_norSB = aviationManager.normalSpeedBoundary;
		_maxSB = aviationManager.maxSpeedBoundary;
	}

    // Update is called once per frame
    void Update()
    {
		controlAngle = aviationManager.controlAngle;
		currForce = thrustControl.currForce;

		switch(status)
		{
			case AviationManager.AviationStatus.BELOW_MINIMUM_SPEED:
				Torque(0);			//비행체 회전하기
				break;

			case AviationManager.AviationStatus.NORMAL_SPEED:
				Torque(1);
				break;

			case AviationManager.AviationStatus.AFTER_BURNER:
				Torque(2);
				break;
		}

		SetTorqueResist();			//회전저항값 구하기

		//print(controlAngle);
		//비행 상태값에 따른 회전각 계산
		//fighterBody.AddRelativeTorque(new Vector3(-(controlAngle.x + 0.4f) * 100000f
		//										, controlAngle.y * 10000f
		//										, -controlAngle.z * 10000)
		//										, ForceMode.Force);
		
    }

	private void SetTorqueResist()
	{
		TorqueResist = Mathf.Abs(controlAngle.x)
					 + Mathf.Abs(controlAngle.y)
					 + Mathf.Abs(controlAngle.z);
	}

	private void Torque(int stat)
	{
		
		switch(stat)
		{
			case 0:

				pitchVal	= -(controlAngle.x + 0.4f) * 50000f
							* (currForce / (_minSB / 5 * 3));
				yawVal		= controlAngle.y * 5000f
							* (currForce / (_minSB / 5 * 4));
				rollVal		= -controlAngle.z * 5000f
							* (currForce / (_minSB / 5 * 3));
				fighterBody.AddRelativeTorque(new Vector3(pitchVal * 1.5f
												, yawVal * 1.5f
                                                , rollVal * 1.5f)
												, ForceMode.Force);
				break;
                //회전가속치 1.5배


			case 1:
                pitchVal = -(controlAngle.x + 0.4f) * 50000f
                            * Mathf.Log((currForce / _minSB) + 1.3f + Mathf.Epsilon, 2);
							//* (_norSB / currForce);
				yawVal		= controlAngle.y * 5000f
							* Mathf.Log((currForce / _minSB) + 1.3f + Mathf.Epsilon, 2);
                rollVal		= -controlAngle.z * 5000f
							* Mathf.Log((currForce / _minSB) + 1.3f + Mathf.Epsilon, 2);
                fighterBody.AddRelativeTorque(new Vector3(pitchVal * 2 * 3
												, yawVal * 2
                                                , rollVal)
												, ForceMode.Force);
				break;
                //회전가속치 2배
                //x 가속치 5배 추가

			case 2:
                float alpha = Mathf.Log(_norSB / _minSB, 2) + 1.3f;
                pitchVal = -(controlAngle.x + 0.4f) * 50000f
                            * alpha - Mathf.Log(alpha - currForce / _norSB);
							//* (_norSB / currForce);
				yawVal = controlAngle.y * 5000f
                            * alpha - Mathf.Log(alpha - currForce / _norSB);
                rollVal = -controlAngle.z * 5000f
                            * alpha - Mathf.Log(alpha - currForce / _norSB);
                fighterBody.AddRelativeTorque(new Vector3(pitchVal * 1.6f * 10
												, yawVal * 1.6f
												, rollVal * 1.6f)
												, ForceMode.Force);
				break;
                //회전가속치 1.6배

		}
	}

	public void GetStatus(AviationManager.AviationStatus stat)
	{
		status = stat;
	}
}