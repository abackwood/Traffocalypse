using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Road : MonoBehaviour {
	public Connection from, to;
	public bool oneWay;
	public int lanes;
	public Lane laneTemplate;
	public float laneMargin;
	public float laneSpacing;

	public Lane[] Lanes { get; private set; }
	public float speedLimit;

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

		Lanes = new Lane[lanes];
		if(oneWay) {
			lanesForward = new Lane[lanes];
			lanesBack = new Lane[0];
			
			for(int i = 0 ; i < lanes ; i++) {
				LaneAnnotation annotation;
				if(i == 0) {
					annotation = LaneAnnotation.RIGHT;
				}
				else if(i == lanes - 1) {
					annotation = LaneAnnotation.LEFT;
				}
				else {
					annotation = LaneAnnotation.CENTER;
				}

				Lane lane = SetupLane(i,annotation,from,to,minOffset);
				lane.SetupLineRenderer();

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
				LaneAnnotation annotation;
				if(j == 0) {
					annotation = LaneAnnotation.RIGHT;
				}
				else if(j == numLanesForward - 1) {
					annotation = LaneAnnotation.LEFT;
				}
				else {
					annotation = LaneAnnotation.CENTER;
				}

				Lane lane = SetupLane(i,annotation,from,to,minOffset);
				lane.SetupLineRenderer();

				Lanes[i] = lane;
				LanesForward[j] = lane;
			}
			for(int j = 0 ; j < lanes - numLanesForward ; i++, j++) {
				LaneAnnotation annotation;
				if(j == lanes - numLanesForward - 1) {
					annotation = LaneAnnotation.RIGHT;
				}
				else if(j == 0) {
					annotation = LaneAnnotation.LEFT;
				}
				else {
					annotation = LaneAnnotation.CENTER;
				}

				Lane lane = SetupLane(i,annotation,to,from,minOffset);
				lane.SetupLineRenderer();

				Lanes[i] = lane;
				LanesBack[j] = lane;
			}
		}
	}
	Lane SetupLane(int id, LaneAnnotation annotation, Connection from, Connection to, float minOffset) {
		Lane lane = GameObject.Instantiate(laneTemplate);
		lane.transform.SetParent(transform);

		lane.road = this;
		lane.id = id;
		lane.annotation = annotation;
		lane.from = from;
		lane.to = to;

		Vector3 direction = this.to.transform.position - this.from.transform.position;	//Direction needs to be constant
		Vector3 ortho = new Vector3(-direction.y, direction.x, 0).normalized;

		lane.startPoint = from.transform.position + (id * laneSpacing - minOffset) * ortho;
		lane.endPoint = to.transform.position + (id * laneSpacing - minOffset) * ortho;

		Vector3 localDirection = lane.to.transform.position - lane.from.transform.position;
		lane.startPoint += localDirection.normalized * laneMargin;
		lane.endPoint -= localDirection.normalized * laneMargin;

		lane.CalculateSpeedAndDirection(speedLimit);

		lane.blocked = false;

		lane.name = lane.ToString();

		return lane;
	}

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public override int GetHashCode ()
	{
		return 331*from.GetHashCode() + to.GetHashCode();
	}

	public override bool Equals (object o)
	{
		if(!o.GetType().Equals (typeof(Road))) {
			return false;
		}
		else {
			Road r = (Road)o;
			return from.Equals (r.from) && to.Equals (r.to);
		}
	}
}
