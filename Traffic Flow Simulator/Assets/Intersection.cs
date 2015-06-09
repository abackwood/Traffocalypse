using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Intersection : Connection {
	public Road[] roads;
	public PossibleTurn[] PossibleTurns { get; private set; }

	// Use this for initialization
	void Start () {
		List<PossibleTurn> list = new List<PossibleTurn>();

		foreach(Road r in roads) {
			list.AddRange (FindPossibleTurns(r));
		}
		PossibleTurns = list.ToArray();

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

	//Returns the list of all possible turns from road r
	//For every lane coming in from r, a possible turn is to an outgoing lane of another road
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
    public Vector3 LineIntersectionPoint(Vector3 ps1, Vector3 pe1, Vector3 ps2,
       Vector3 pe2)
    {
        // Get A,B,C of first line - points : ps1 to pe1
        float A1 = pe1.y - ps1.y;
        float B1 = ps1.x - pe1.x;
        float C1 = A1 * ps1.x + B1 * ps1.y;

        // Get A,B,C of second line - points : ps2 to pe2
        float A2 = pe2.y - ps2.y;
        float B2 = ps2.x - pe2.x;
        float C2 = A2 * ps2.x + B2 * ps2.y;

        // Get delta and check if the lines are parallel
        float delta = A1 * B2 - A2 * B1;
        if (delta == 0)
            throw new System.Exception("Lines are parallel");

        // now return the Vector3 intersection point
        return new Vector3(
            (B2 * C1 - B1 * C2) / delta,
            (A1 * C2 - A2 * C1) / delta,
            0
        );
    }
	
	// Update is called once per frame
	void Update () {
		
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

	public PossibleTurn(Lane laneIn, Lane[] lanesOut) {
		this.laneIn = laneIn;
		this.lanesOut = lanesOut;
	}
}

public struct ExplicitTurn {
	PossibleTurn turn;
	int index;

	public Intersection Intersection {
		get { return (Intersection)LaneIn.To; }
	}
	public Lane LaneIn {
		get { return turn.LaneIn; }
	}
	public Lane LaneOut {
		get { return turn.LanesOut[index]; }
	}

	public ExplicitTurn(PossibleTurn turn, int index) {
		this.turn = turn;
		this.index = index;
	}

	public override string ToString ()
	{
		return "(" + LaneIn + " > " + LaneOut + ")";
	}
}