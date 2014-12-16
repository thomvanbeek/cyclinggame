using UnityEngine;                        // These are the librarys being used
using System.Collections;
using System;
using System.IO;
using System.Net.Sockets; 

public class ClientSocket : MonoBehaviour {
	
	bool socketReady = false;                // global variables are setup here
	TcpClient mySocket;
	public NetworkStream theStream;
	StreamWriter theWriter;
	StreamReader theReader;
	public String Host = "INSERT the public IP of router or Local IP of Arduino";
	public Int32 Port = 5001; 
	public String lineRead;
	int angle = -30;
	
	
	void Start() {
		setupSocket (); 
		StartCoroutine(Run());

	// setup the server connection when the program starts
	}

	IEnumerator Run()
	{

		while (true) {
			lineRead = readSocket();
			yield return new WaitForSeconds(0.1f);
		} 

	}

	
	// Update is called once per frame
	void Update() {
		                 // if new data is recieved from Arduino
		//string recievedData = readSocket();            // write it to a string
		

	}
	
	public void setupSocket() {                            // Socket setup here
		try {                
			mySocket = new TcpClient(Host, Port);
			theStream = mySocket.GetStream();
			theWriter = new StreamWriter(theStream);
			theReader = new StreamReader(theStream);
			socketReady = true;
		}
		catch (Exception e) {
			Debug.Log("Socket error:" + e);                // catch any exceptions
		}
	}
	
	public void writeSocket(string theLine) {            // function to write data out
		if (!socketReady)
			return;
		String tmpString = theLine;
		theWriter.Write(tmpString);
		theWriter.Flush();
		
		
	}
	
	public String readSocket() {// function to read data in
		if (!socketReady) {
			angle++;
						return "<data><angle>" + angle + "</angle><speed>3</speed><brake>0</brake></data>";
				}
		if (theStream.DataAvailable)
			return theReader.ReadLine();
		return "NoData";
	}
	
	public void closeSocket() {                            // function to close the socket
		if (!socketReady)
			return;
		theWriter.Close();
		theReader.Close();
		mySocket.Close();
		socketReady = false;
	}
	
	public void maintainConnection(){                    // function to maintain the connection (not sure why! but Im sure it will become a solution to a problem at somestage)
		if(!theStream.CanRead) {
			setupSocket();
		}
	}
	
	
}