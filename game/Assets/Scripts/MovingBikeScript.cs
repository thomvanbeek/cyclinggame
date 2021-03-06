﻿using UnityEngine;
using System.Collections;

public class MovingBikeScript : MonoBehaviour {
	
	public Rigidbody frontWheel;
	public Rigidbody rearWheel;
	public Rigidbody frame;

	public TextGUIScript TextGUI;
	
	public Transform testSphere;
	public Transform testSphereRear;
	
	private float rotation = 0;
	private float rotationUnit = 0.5f;
	// Use this for initialization
	void Start () {
		rotation = 0;
	}
	
	void Update () {
	}
	
	void FixedUpdate () {
		
		float h = Input.GetAxis ("Horizontal");
		float v = Input.GetAxis ("Vertical");
		//float r = Input.GetAxis ("Rotation");

		/*if (h > 0 && frontWheel.rotation.y < 0.5 || h < 0 && frontWheel.rotation.y > -0.5 ) {
				frontWheel.AddTorque(0,h/5,0);	
		}*/
		
		//Debug.Log (frontWheel.rotation);

		if(h != 0) {
			if (h > 0) {
				if(rotation > 0) {
					rotation += 1*rotationUnit;
				} else {
					rotation += 3*rotationUnit;
				}
			} else {
				if(rotation < 0) {
					rotation -= 1*rotationUnit;
				} else {
					rotation -= 3*rotationUnit;
				}
			}
		}

		//Vector3 vectorBike = Quaternion.Euler (frame.rotation.eulerAngles) * Vector3.forward;
		Vector3 vector = Quaternion.Euler(0,rotation,0) * Quaternion.Euler(frame.rotation.eulerAngles) * Vector3.forward;
		
		//testSphere.position = frontWheel.transform.position + vector;

		//testSphereRear.position = frontWheel.transform.position + vectorBike;

		frontWheel.rotation = Quaternion.LookRotation(vector);

		if (v != 0) {
			rearWheel.AddRelativeTorque(50000000*v,0,0);	
		}
	}
	
	void LateUpdate () {
//		TextGUI.UpdateBikeValuesText (rotation, frame.rigidbody.velocity.magnitude);
	}
}
