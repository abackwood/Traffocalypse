using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Road : MonoBehaviour {
	public Connection from, to;
	public bool oneWay;
	public int lanes;
	public Lane laneTemplate;
	public float laneSpacing;

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
		transform.position = (from.transform.position + to.transform.position) / 2;

		float minOffset = (laneSpacing * (lanes - 1)) / 2;	//Half of the total offset between lanes

		Lanes = new Lane[lanes];	//TODO Figure out where line renderer and start/end of lanes should be set
		if(oneWay) {
			lanesForward = new Lane[lanes];
			lanesBack = new Lane[0];
			
			for(int i = 0 ; i < lanes ; i++) {
				Lane lane = SetupLane(i,from,to);
				//SetupLineRenderer(lane,i,minOffset);

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
				Lane lane = SetupLane(i,from,to);
				//SetupLineRenderer(lane,i,minOffset);

				Lanes[i] = lane;
				LanesForward[j] = lane;
			}
			for(int j = 0 ; j < lanes - numLanesForward ; i++, j++) {
				Lane lane = SetupLane(i,to,from);
				//SetupLineRenderer(lane,i,minOffset);

				Lanes[i] = lane;
				LanesBack[j] = lane;
			}
		}
	}
	Lane SetupLane(int id, Connection from, Connection to) {
		Lane lane = GameObject.Instantiate(laneTemplate);
		lane.transform.SetParent(transform);

		lane.road = this;
		lane.id = id;
		lane.from = from;
		lane.to = to;

		return lane;
	}
	void SetupLineRenderer(Lane lane, int i, float minOffset) {
		Vector3 direction = to.transform.position - from.transform.position;
		Vector3 ortho = new Vector3(-direction.y, direction.x, 0).normalized;

		LineRenderer renderer = lane.GetComponent<LineRenderer>();
		Vector3 offset_from = from.transform.position + (i * laneSpacing - minOffset) * ortho;
		Vector3 offset_to = to.transform.position + (i * laneSpacing - minOffset) * ortho;

		lane.startPoint = offset_from;
		lane.endPoint = offset_to;

		renderer.SetPosition(0,offset_from);
		renderer.SetPosition(1,offset_to);
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

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
