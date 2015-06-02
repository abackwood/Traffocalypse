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
				Lane lane = new Lane(this,i,from,to);
				Lanes[i] = lane;
				LanesForward[j] = lane;
			}
			for(int j = 0 ; j < lanes - numLanesForward ; i++, j++) {
				Lane lane = new Lane(this,i,to,from);
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
	void Start () {
        Instantiate(rightLane, new Vector3(0, 0, 0), Quaternion.identity);
        Instantiate(leftLane, new Vector3(0, 0, 0), Quaternion.identity);

        float xDifference = from.transform.position.x - to.transform.position.x;
        float yDifference = from.transform.position.y - to.transform.position.y;
        length = Mathf.Sqrt((xDifference * xDifference) + (yDifference * yDifference));
        float xNorm = xDifference / length*5;
        float yNorm = yDifference / length*5;

        Vector3 fromPos = from.transform.position;
        fromPos.y += xNorm;
        fromPos.x -= yNorm;
        Vector3 toPos = to.transform.position;
        toPos.y += xNorm;
        toPos.x -= yNorm;

		LineRenderer rendererRight = rightLane.GetComponent<LineRenderer>();
        LineRenderer rendererLeft = leftLane.GetComponent<LineRenderer>();

		float width = 1f;
		rendererRight.SetWidth(width,width);
        rendererLeft.SetWidth(width, width);

		rendererRight.SetPosition(0,fromPos);
		rendererRight.SetPosition(1,toPos);

        fromPos.y -= xNorm;
        fromPos.x += yNorm;
        toPos.y -= xNorm;
        toPos.x += yNorm;

        rendererLeft.SetPosition(0, fromPos);
        rendererLeft.SetPosition(1, toPos);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
