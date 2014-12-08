using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

public class MovingBikeScript : MonoBehaviour {
	
	public Rigidbody frontWheel;
	public Rigidbody rearWheel;
	public Rigidbody frame;

	public TextAsset GameAsset;


	public GUIText rotationText;
	public GUIText speedText;
	
	public Transform testSphere;
	public Transform testSphereRear;
	
	private float rotation = 0;
	private float rotationUnit = 0.5f;

	private float speed = 0;
	private float angle = 0;
	private int brake = 0;
	// Use this for initialization
	void Start () {
		rotation = 0;
	}
	
	void Update () {
	}
	
	void FixedUpdate () {

		GetData ();
		
		float h = Input.GetAxis ("Horizontal");
		float v = Input.GetAxis ("Vertical");
		//float r = Input.GetAxis ("Rotation");
		
		//Debug.Log (frontWheel.rotation);
		//Vector3 vectorBike = Quaternion.Euler (frame.rotation.eulerAngles) * Vector3.forward;
		//Vector3 vector = Quaternion.Euler(0,rotation,0) * Quaternion.Euler(frame.rotation.eulerAngles) * Vector3.forward;
		
		//testSphere.position = frontWheel.transform.position + vector;

		//testSphereRear.position = frontWheel.transform.position + vectorBike;

		//frontWheel.rotation = Quaternion.LookRotation(vector);
		Debug.Log (angle);

		float offsetAngle = frame.rotation.eulerAngles.y;
		frontWheel.transform.eulerAngles = new Vector3(0, offsetAngle +  angle, 0);


		//rearWheel.AddRelativeTorque(50000000*speed,0,0);	
		rearWheel.transform.Rotate(speed * 100,0,0);
	}


	public void GetData()
	{
		XmlDocument xmlDoc = new XmlDocument(); // xmlDoc is the new xml document.
		xmlDoc.LoadXml(GameAsset.text); // load the file.
		XmlNodeList levelsList = xmlDoc.GetElementsByTagName("data"); // array of the level nodes.
		
		foreach (XmlNode levelInfo in levelsList)
		{
			XmlNodeList levelcontent = levelInfo.ChildNodes;			
			foreach (XmlNode levelsItens in levelcontent) // levels itens nodes.
			{
				if(levelsItens.Name == "speed")
				{
					speed = float.Parse(levelsItens.InnerText);
				}
				
				if(levelsItens.Name == "angle")
				{
					angle = float.Parse(levelsItens.InnerText);
				}
				
				if(levelsItens.Name == "brake")
				{
					brake =  int.Parse(levelsItens.InnerText);
				}
			}
		}
	}

	void LateUpdate () {
		rotationText.text = "Rotation : " + angle + "º";
		speedText.text = "Speed : " + frame.rigidbody.velocity.magnitude.ToString();
	}
}
