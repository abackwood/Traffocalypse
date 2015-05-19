using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (LineRenderer))]
public class Road : MonoBehaviour {
	public Connection from, to;
	public bool oneWay;
	public int lanes;

	public Lane[] Lanes { get; private set; }

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

	// Use this for initialization
	void Start () {
		LineRenderer renderer = GetComponent<LineRenderer>();

		float width = 1f;
		renderer.SetWidth(width,width);

		renderer.SetPosition(0,from.transform.position);
		renderer.SetPosition(1,to.transform.position);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

public struct Lane {
	Road road;
	public Road Road {
		get { return road; }
	}

	int id;
	public int ID {
		get { return id; }
	}

	Connection from;
	public Connection From {
		get { return from; }
	}

	Connection to;
	public Connection To {
		get { return to; }
	}

	List<Car> carsOnLane;
	public Car[] CarsOnLane {
		get { return carsOnLane.ToArray(); }
	}

	public Lane(Road road, int id, Connection from, Connection to) {
		this.road = road;
		this.id = id;
		this.from = from;
		this.to = to;

		carsOnLane = new List<Car>();
	}

	public override string ToString ()
	{
		return Road.name + "-" + id + "(" + from.name + "-" + to.name + ")";
	}
}