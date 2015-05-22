using UnityEngine;
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
