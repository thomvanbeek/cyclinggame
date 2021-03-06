﻿using UnityEngine;
using System.Collections;

public class BikePhysicsScript : MonoBehaviour
{
	Socket Network;

    public GameObject centerOfMass;
    public GameObject frontWheel;
    public GameObject rearWheel;
	public Transform modelFrontFork;

    public TextGUIScript TextGUI;
	public Trail trailScript;

    public float gravity;
	public float multHelpToStabilize;
	public bool canRoll;
	public bool useNetwork;
	public bool useSteeringSuggestions;

	public GameObject picLeft;
	public GameObject picRight;
	public GameObject picForward;
	private bool displayingPics = false;

    private float forkRotation;
    private float speed;

	private float rotRadius;

    private float rollAngularSpeed;
    private float rollAngularAcc;
    private float rotAngularSpeed;

	private const float maxSteerRotation = 80.0f;
	private const float minSteerRotation = -80.0f;

    //TODO delete this!
    private float rotationUnit = 0.5f;
    private float speedUnit = 0.08f;
	private float brakeUnit = 0.5f;

	public TextGUIScript GUIscript;

	private bool dying = false;


	private bool youHaveDied = false;

    // Use this for initialization
    void Start()
    {
		rotRadius = 0;
        rollAngularSpeed = 0.0f;
        rollAngularAcc = 0.0f;

        forkRotation = 0;
        speed = 0;

		Network = GameObject.Find ("Network").GetComponent<Socket> ();
    }

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
			canRoll = !canRoll;
		if (Input.GetKey (KeyCode.Plus))
			gravity = gravity + 0.25f;
		if (Input.GetKey (KeyCode.Minus))
			gravity = gravity - 0.25f;
	}

    // Update is called once per frame
    void FixedUpdate()
    {
        if (MenuSelection.state != GameState.Playing)
            return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

		float f = Input.GetAxis("Fire1");
		


		
		if(f ==1) {
			ResetBike();
			trailScript.Reset();
		}

		if(useNetwork) {
			forkRotation = ApplyMaxMinRotation(Network.getParsedAngle());
	  
			float networkSpeed = Network.getParsedSpeed ();

			if (speed < networkSpeed)
				speed = speed + speedUnit;
			else //if (speed > networkSpeed)
				speed = speed - speedUnit;

			if (Network.getParsedBrake() == 1) {
				speed = speed - brakeUnit;
				if (speed < 0)
					speed = 0;
				}
		} else {
			UpdateRotation(h);
			UpdateSpeed(v);
		}


        // Moving and rotation part
        //float angleSpeed = Mathf.Abs(forkRotation);

        Vector3 vector = Quaternion.Euler(0, this.transform.rotation.y, 0) * Quaternion.Euler(this.transform.rotation.eulerAngles) * Vector3.forward;

        Quaternion angle;

		if (rotAngularSpeed > 0) {
			angle = Quaternion.Euler (0, -1, 0) * this.transform.rotation;
			rotAngularSpeed= -rotAngularSpeed;
		}
		else 
			angle = Quaternion.Euler (0, 1, 0) * this.transform.rotation;
		
		this.transform.position = Vector3.MoveTowards(this.transform.position,
                                                      this.transform.position + vector,
                                                      speed * Time.deltaTime);

		Debug.Log (transform.position.y);
//			Vector3 pos = this.transform.position;		
//
//			pos.y = 0;
//
//			this.transform.position = pos;

        this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation,
                                                           angle,
                                                           rotAngularSpeed);

        //Rolling and gravity part
        ApplyGravity(); //makes sure the wheels are against the floor.. maybe it can be removed
		ApplyRotation(); //makes the bike to steer
        ApplyInstability(); //if the center of mass isn't over the wheels, it will fall

        modelFrontFork.localRotation = Quaternion.AngleAxis(forkRotation, new Vector3(0, 3, -1));
   
		if (this.transform.rotation.eulerAngles.z > 85 && this.transform.rotation.eulerAngles.z < 275) {
			this.transform.rotation = Quaternion.identity;
			StartCoroutine ("dieFunction");
		
		}
		  
	}

    void ApplyGravity()
    {

		if(transform.position.y > 0.5 || transform.position.y < -0.5) 
		{
			Vector3 vector = new Vector3(0,0,0);
			vector.x = transform.position.x;
			vector.z = transform.position.z;
			transform.position = Vector3.MoveTowards(this.transform.position,
			                                         vector,
			                                         Time.deltaTime);
			Vector3 vector2 = new Vector3(0,0,0);
			vector2.y = transform.localRotation.eulerAngles.y;
			vector2.z = transform.localRotation.eulerAngles.z;
			transform.localRotation = Quaternion.Euler(vector2);
		}
        
        /*if (frontWheel.transform.position.y != 0)
        {
            Vector3 vector;
            if (frontWheel.transform.position.y < 0)
            {
                vector = Vector3.up;
            }
            else
            {
                vector = Vector3.down;
            }
            vector *= Mathf.Abs(frontWheel.transform.position.y);
            this.transform.position = Vector3.MoveTowards(this.transform.position,
                                                          this.transform.position + vector,
                                                          2 * Time.deltaTime);
        }*/
    }

    void ApplyInstability()
    {
		rollAngularAcc = 0;

        if (this.transform.rotation.eulerAngles.z != 0)
        {
            rollAngularAcc = gravity * Mathf.Sin(Mathf.Deg2Rad * this.transform.rotation.eulerAngles.z) / centerOfMass.transform.position.y;
        }

		if(rotRadius != 0) {
			rollAngularAcc +=speed*speed / (rotRadius * centerOfMass.transform.position.y * centerOfMass.transform.position.y);

		}

        if (rollAngularAcc != 0)
        {
            rollAngularSpeed += rollAngularAcc * Time.deltaTime;
        }

		//Debug.Log(rollAngularAcc);
		if(multHelpToStabilize > 1) {

			if((rollAngularAcc > 0 && this.transform.rotation.eulerAngles.z > 180 && this.transform.rotation.eulerAngles.z < 360) ||
			   (rollAngularAcc < 0 && this.transform.rotation.eulerAngles.z < 180 && this.transform.rotation.eulerAngles.z > 0)) {
				rollAngularAcc *= multHelpToStabilize;
			}
		}

		//TODO: improve this
        if (rollAngularSpeed != 0 && canRoll)
        {
            Vector3 angleEuler = this.transform.rotation.eulerAngles;
            angleEuler.z += 1;
            Quaternion angle = Quaternion.Euler(angleEuler);

            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation,
                                                               angle,
			                                                   rollAngularSpeed);

			if (useSteeringSuggestions)
			{
				if(rollAngularSpeed > 0.3 && forkRotation > 0) {
					displayingPics = true;
					picLeft.SetActive(true);
					picRight.SetActive (false);
                    
                } else if(rollAngularSpeed < - 0.3 && forkRotation < 0) {
					displayingPics = true;
					picRight.SetActive (true);
					picLeft.SetActive (false);
                    
                } else {
					displayingPics = false;
					picLeft.SetActive (false);
					picRight.SetActive (false);
				}
				/*
				if (rollAngularSpeed > 0.5 && forkRotation > 0 && !displayingPics)
				{
					StartCoroutine("suggestTurnLeft");	
				}

				else if (rollAngularSpeed < - 0.5 && forkRotation < 0 && !displayingPics)
				{
					StartCoroutine("suggestTurnRight");
				}
				*/
			}
        }
    }

		IEnumerator suggestTurnLeft() {
				Time.timeScale = 0.1f;
				displayingPics = true;
				picForward.SetActive (true);
				yield return new WaitForSeconds (0.05f);
				picForward.SetActive (false);
				picLeft.SetActive (true);
				yield return new WaitForSeconds (0.05f);
				picLeft.SetActive (false);
				yield return new WaitForSeconds (0.05f);
				picForward.SetActive (false);
				picLeft.SetActive (true);
				yield return new WaitForSeconds (0.05f);
				picLeft.SetActive (false);
				yield return new WaitForSeconds (0.02f);
				Time.timeScale = 1;
				yield return new WaitForSeconds (1);
				displayingPics = false;
			}

		IEnumerator suggestTurnRight() {
			Time.timeScale = 0.1f;
			displayingPics = true;
			picForward.SetActive (true);
			yield return new WaitForSeconds (0.05f);
			picForward.SetActive (false);
			picRight.SetActive (true);
			yield return new WaitForSeconds (0.05f);
			picRight.SetActive (false);
			yield return new WaitForSeconds (0.05f);
			picForward.SetActive (false);
			picRight.SetActive (true);
			yield return new WaitForSeconds (0.05f);
			picRight.SetActive (false);
			yield return new WaitForSeconds (0.02f);
			Time.timeScale = 1;
			yield return new WaitForSeconds (1);
			displayingPics = false;
		}

    void ApplyRotation()
    {

        float difWheel = frontWheel.transform.localPosition.z;
        if (forkRotation != 0)
        {
			rotRadius = difWheel / Mathf.Tan(forkRotation * Mathf.Deg2Rad);
        } else {
			rotRadius = 0;
		}

		if (rotRadius != 0)
        {
			rotAngularSpeed = speed / rotRadius;
        } else {
			rotAngularSpeed = 0;
		}
    }

	void OnCollisionEnter (Collision col)
	{
		if (col.collider.gameObject.name.Equals ("Cube") && !dying) {
			StartCoroutine("dieFunction");
		}
	}

	IEnumerator dieFunction()
	{
		dying = true;
		if (MenuSelection.state == GameState.Playing) {
						Color screenTint = new Color (1, 0, 0, 1f);
						Color noTint = new Color (0.2f, 0.2f, 0.2f, 1f);
						RenderSettings.ambientLight = screenTint;
//		RenderSettings.fogColor = screenTint;
//		RenderSettings.fogDensity = .9f;
						//GUIscript.DisplayMessage ("WASTED", Color.red);

						MenuSelection menuObject = GameObject.Find ("Third Person Camera").GetComponent<MenuSelection> ();
						menuObject.setYouHaveDied (true);


						Time.timeScale = 0.5f;
						yield return new WaitForSeconds (1f);
						Time.timeScale = 1f;
						if (MenuSelection.substate == SubGameState.Free)
								menuObject.StartNewGame ();
						else {
								GeneralController.addScoreRace ("" + GeneralController.score);
								MenuSelection.state = GameState.Highscores;
						}


						menuObject.setYouHaveDied (false);
						RenderSettings.ambientLight = noTint;
				}
		dying = false;
	}

    void UpdateRotation(float rot)
    {
        if (rot != 0)
        {
			if (rot > 0 && forkRotation < maxSteerRotation)
            {
                if (forkRotation > 0)
                {
                    forkRotation += 1 * rotationUnit;
                }
                else
                {
                    forkRotation += 3 * rotationUnit;
                }
            }
            else
            {
				if (forkRotation > minSteerRotation)
				{
	                if (forkRotation < 0)
	                {
	                    forkRotation -= 1 * rotationUnit;
	                }
	                else
	                {
	                    forkRotation -= 3 * rotationUnit;
	                }
				}
            }
        }
    }

	float ApplyMaxMinRotation(float rotation)
	{
		if (rotation > maxSteerRotation)
						return maxSteerRotation;
				else if (rotation < minSteerRotation)
						return minSteerRotation;
				else
						return rotation;
	}

    void UpdateSpeed(float force)
    {
        if (force != 0)
        {
            speed += force * speedUnit;
        }
    }

	public void ResetBike() {
		Rigidbody rigid = gameObject.GetComponent<Rigidbody> ();
		if (rigid != null) {
			rigid.angularVelocity = Vector3.zero;
			rigid.velocity = Vector3.zero;
		}

		ResetBikeRoll();
		ResetBikePos();
		ResetBikeSteerAngle();
		ResetBikeSpeed();
	}

	void ResetBikeRoll() {
		rollAngularSpeed = 0.0f;
		Vector3 angleEuler = this.transform.rotation.eulerAngles;
		angleEuler.z = 0;
		Quaternion angle = Quaternion.Euler(angleEuler);
		
		this.transform.rotation = angle;
	}

	void ResetBikePos() {
		this.transform.position = new Vector3 (0, 0, 0);
		this.transform.rotation = Quaternion.Euler (new Vector3 (0, 0, 0));
	}

	void ResetBikeSpeed() {
		speed = 0.0f;
	}

	void ResetBikeSteerAngle() {
		forkRotation = 0.0f;
	}

	public float GetSpeed() {
		return speed;
	}

    void LateUpdate()
    {
        //TextGUI.UpdateBikeValuesText(forkRotation, speed);
    }
}
