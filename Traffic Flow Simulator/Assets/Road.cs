using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Road : MonoBehaviour {
	public Connection from, to;
	public bool oneWay;
	public int lanes;
    public Transform rightLane;
    public Transform leftLane;

	public Lane[] Lanes { get; private set; }

	public float length;
	public float speed;
	public float xSpeed;
	public float ySpeed;
	
	public float SpeedX
	{
		get { return xSpeed;}
	}
	
	public float SpeedY
	{
		get { return ySpeed; }
	}
    
	Lane[] lanesForward, lanesBack;
	public Lane[] LanesForward {
		get {
			if(lanesForward == null) {
				InitRoad ();
			}
			return lanesForward;
		}
	}
	public Lane[] LanesBack {
		get {
			if(lanesBack == null) {
				InitRoad ();
			}
			return lanesBack;
		}
	}
    
	public Lane[] InLanes(Connection c) {
		if(from == c) return LanesBack;
		else if(to == c) return LanesForward;
		else return null;
	}

	public Lane[] OutLanes(Connection c) {
		if(from == c) return LanesForward;
		else if(to == c) return LanesBack;
		else return null;
	}

    
	void InitRoad() {
		Lanes = new Lane[lanes];
		if(oneWay) {
			lanesForward = new Lane[lanes];
			lanesBack = new Lane[0];
			
			for(int i = 0 ; i < lanes ; i++) {
				Lane lane = new Lane(this,i,from,to);
				Lanes[i] = lane;
				lanesForward[i] = lane;
			}
		}
		else {
			int numLanesForward = lanes / 2;
			lanesForward = new Lane[numLanesForward];
			lanesBack = new Lane[lanes - numLanesForward];
			
			int i = 0;
			for(int j = 0 ; j < numLanesForward ; i++, j++) {
				//Lane lane = new Lane(this,i,from,to);
                Debug.Log("TEEEASFAVADF");
                Instantiate(leftLane, new Vector3(0, 0, 0), Quaternion.identity);
                Lane lane = leftLane.GetComponent<Lane>();
                lane.Initiate(this, i, from, to);
				Lanes[i] = lane;
				LanesForward[j] = lane;
			}
			for(int j = 0 ; j < lanes - numLanesForward ; i++, j++) {
				//Lane lane = new Lane(this,i,to,from);
                Instantiate(rightLane, new Vector3(0, 0, 0), Quaternion.identity);
                Lane lane = rightLane.GetComponent<Lane>();
                lane.Initiate(this, i, to, from);
				Lanes[i] = lane;
				LanesBack[j] = lane;
			}
		}

	}

	void InitLanes() {
		
	}

	void CalculateSpeed()
	{
		float xDifference = Mathf.Abs(from.transform.position.x - to.transform.position.x);
		float yDifference = Mathf.Abs(from.transform.position.y - to.transform.position.y);
		length = Mathf.Sqrt((xDifference * xDifference) + (yDifference * yDifference));
		xSpeed = (xDifference * speed) / length;
		ySpeed = (yDifference * speed) / length;
	}

	// Use this for initialization
	void Start () 
    {
        //InitRoad();
        //Instantiate(rightLane, new Vector3(0, 0, 0), Quaternion.identity);
        //Instantiate(leftLane, new Vector3(0, 0, 0), Quaternion.identity);
        //Lane rightLaneScript = rightLane.GetComponent<Lane>();
        //rightLaneScript.Initiate(this, 0, to, from);
        //Lane leftLaneScript = leftLane.GetComponent<Lane>();
        //leftLaneScript.Initiate(this, 1, from, to);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
