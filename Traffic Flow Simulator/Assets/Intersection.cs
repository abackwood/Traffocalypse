using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Intersection : Connection {
	public Road[] roads;
	public TrafficAgent agent;

	public PossibleTurn[] PossibleTurns { get; private set; }
	public List<PossibleTurn> OpenTurns { private get; set; }

	// Use this for initialization
	void Start () {
		List<PossibleTurn> list = new List<PossibleTurn>();

		foreach(Road r in roads) {
			list.AddRange (FindPossibleTurns(r));
		}
		PossibleTurns = list.ToArray();

		OpenTurns = new List<PossibleTurn>(PossibleTurns);

		string s = name + " possible turns: ";
		foreach(PossibleTurn pt in PossibleTurns) {
			s += "(" + pt.LaneIn + " -> [";
			foreach(Lane lOut in pt.LanesOut) {
				s += lOut + " ";
			}
			s += "]) ";
		}
		Debug.Log (s);
	}

	/// <summary>
	/// Returns all possible turns that can be made from this road at this intersection.
	/// A possible turn is from an incoming lane on r to all outgoing lanes of another road.
	/// </summary>
	/// <returns>The list of possible turns for this intersection.</returns>
	/// <param name="r">The road.</param>
	List<PossibleTurn> FindPossibleTurns(Road r) {
		List<PossibleTurn> list = new List<PossibleTurn>();
		foreach(Road r1 in roads) {
			if(r != r1) {
				foreach(Lane l in r.InLanes(this)) {
					list.Add (new PossibleTurn(l, r1.OutLanes(this)));
				}
			}
		}
		return list;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public bool IsOpen(PossibleTurn turn) {
		return OpenTurns.Contains(turn);
	}
}

public struct PossibleTurn {
	Lane laneIn;
	public Lane LaneIn {
		get { return laneIn; }
	}

	Lane[] lanesOut;
	public Lane[] LanesOut {
		get { return lanesOut; }
	}

	Vector3[] laneTurnPoints;
	public Vector3[] LaneTurnPoints {
		get { return laneTurnPoints; }
	}

	public PossibleTurn(Lane laneIn, Lane[] lanesOut) {
		this.laneIn = laneIn;
		this.lanesOut = lanesOut;

		laneTurnPoints = new Vector3[lanesOut.Length];
		for(int i = 0 ; i < lanesOut.Length ; i++) {
			Vector2 intersection = LaneIntersectionPoint(laneIn, lanesOut[i]);
			laneTurnPoints[i] = new Vector3(intersection.x, intersection.y, 0);
		}
	}

	Vector2 LaneIntersectionPoint(Lane inLane, Lane outLane) {
		Vector2 in_start = inLane.startPoint;
		Vector2 in_end = inLane.endPoint;
		Vector2 out_start = outLane.startPoint;
		Vector2 out_end = outLane.endPoint;

		// Get A,B,C of first line
		float A1 = in_end.y-in_start.y;
		float B1 = in_start.x-in_end.x;
		float C1 = A1*in_start.x+B1*in_start.y;
		
		// Get A,B,C of second line
		float A2 = out_end.y-out_start.y;
		float B2 = out_start.x-out_end.x;
		float C2 = A2*out_start.x+B2*out_start.y;
		
		// Get delta and check if the lines are parallel
		float delta = A1*B2 - A2*B1;
		if(delta == 0) {
			return (in_end - out_start) / 2;
		}
		
		// now return the Vector2 intersection point
		return new Vector2(
			(B2*C1 - B1*C2) / delta,
			(A1*C2 - A2*C1) / delta);
	}
}

public struct ExplicitTurn {
	PossibleTurn turn;
	int index;

	public Intersection Intersection {
		get { return (Intersection)LaneIn.to; }
	}
	public PossibleTurn Parent {
		get { return turn; }
	}
	public Lane LaneIn {
		get { return turn.LaneIn; }
	}
	public Lane LaneOut {
		get { return turn.LanesOut[index]; }
	}
	public Vector3 TurnPoint {
		get { return turn.LaneTurnPoints[index]; }
	}

	public ExplicitTurn(PossibleTurn turn, int index) {
		this.turn = turn;
		this.index = index;
	}

	public override string ToString ()
	{
		return "(" + LaneIn + " -(" + TurnPoint + ")->" + LaneOut + ")";
	}
}