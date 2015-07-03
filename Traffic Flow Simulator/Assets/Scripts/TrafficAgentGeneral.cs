using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * Our algorithm for handling traffic lights relies on the intuition that any two turns can be active
 * at the same time if they don't get in each other's way. An intersection with arbitrary amount of roads can
 * be seen as a circle of roads, between which turns can happen. Assuming right-handed traffic, all turns occupy
 * a counter-clockwise arc of this circle (from source to destination). The arc of two turns in the same phase may
 * not overlap, for that would mean they get in each other's way. This also explains why left turns are awkward. They occupy a
 * relatively large part of the circle, thus blocking many other turns.
 * 
 * This idea is defined formally below:
 * 
 * Let R be a circular array of indexed roads {1...n} so that R[k] = R[k+n].
 * Turn (i,j) is defined as the turn from R[i] to R[j].
 * Let T be the set of all turns that can be made at the intersection.
 * A phase P is a subset of T, so that for all (i1,j1),(i2,j2) in P, with (i1,j1) =/= (i2,j2),
 * (i1,j1) and (i2,j2) are not in conflict (see below).
 * 
 * (i1,j1) and (i2,j2) are in conflict if i1 =/= i2 AND i2 < i1 and j2 > i1 OR i2 > i1 and i2 < j1.
 * (The constraint "i1 =/= i2" represents the assumption that cars coming from the same road can never collide and their turns
 * are thus never conflicting)
 * 
 * The aim is to find a phase P* that maximises the sum value of some valuation function F:(i,j) -> R over turns.
 * This makes this problem similar to the knapsack problem. Unsurprisingly, it is also NP-hard. Calculating the optimal phase
 * at any point in time would be infeasible. Therefore we settle for a phase P that approximates P* with a greedy approach:
 * 
 * PRECONDITION: T = set of all turns
 * P <- {}
 * S <- T
 * WHILE S is not empty
 * 		(i,j) <- ARGMAX(S) for F
 * 		Add (i,j) to P
 * 		Remove (i,j) from S
 * 		FORALL (i1,j1) in S
 * 			IF (i,j) in conflict with (i1,j1) THEN
 * 				Remove (i1,j1) from S
 * Return P.
 * 
 * We only need to check the newly added turn for conflicts because by definition of the algorithm, any turns
 * conflicting with previous turns in P would have been deleted.
 * 
 * This algorithm has one problem: in itself it makes no guarantee that any one turn will ever be added to the phase.
 * This translates to a perpetual red light. This is obviously not acceptable. To resolve this, the function F assigns a higher
 * value to a turn the longer it has had a red light. At some point, the value will grow high enough that the greedy approach chooses it.
 * This gives us the desired liveness property that every turn will eventually be given a green light.
 * 
 * Function F also assigns a higher value to roads with a higher amount of cars waiting, so priority is given to potential turns that could
 * resolve this.
 */

public class TrafficAgentGeneral : MonoBehaviour {
	public Intersection intersection;
	public float phaseLength;
	public float timeValueRatio;
	
	Turn[] allTurns;
	Road[] roads;
	HashSet<Turn> currentPhase;
	float timeSincePhaseStart;

	public bool IsOpen(PossibleTurn possibleTurn) {
		if(currentPhase == null) {
			currentPhase = ChooseNextPhase();
			timeSincePhaseStart = 0;
		}

		Road inRoad = possibleTurn.LaneIn.road;
		Road outRoad = possibleTurn.LanesOut[0].road;
		foreach(Turn t in currentPhase) {
			if(roads[t.in_idx].Equals (inRoad) &&
			   roads[t.out_idx].Equals (outRoad)) {
				//Debug.Log (t + " is open");
				return true;
			}
		}
		return false;
	}

	// Use this for initialization
	void Start () {
		roads = intersection.roads;
		allTurns = FindAllTurns();
	}

	Turn[] FindAllTurns() {
		List<Turn> turnList = new List<Turn>();
		for(int i = 0 ; i < roads.Length ; i++) {
			Road r1 = roads[i];
			if(r1.InLanes(intersection).Length > 0) {
				for(int j = 0 ; j < roads.Length ; j++) {
					Road r2 = roads[j];
					if(r1 != r2 && r2.OutLanes(intersection).Length > 0) {
						turnList.Add (new Turn(i,j));
					}
				}
			}
		}
		return turnList.ToArray();
	}
	
	// Update is called once per frame
	void Update () {
		timeSincePhaseStart += Time.deltaTime;
		if(timeSincePhaseStart > phaseLength) {
			currentPhase = ChooseNextPhase();
			timeSincePhaseStart = 0;
		}
	}

	HashSet<Turn> ChooseNextPhase() {
		HashSet<Turn> phase = new HashSet<Turn>();
		List<Turn> bag = new List<Turn>(allTurns);

		while(bag.Count > 0) {
			Turn best = FindMostValuableTurn(bag);

			string s1 = name + " Bag: {";
			foreach(Turn turn in bag) {
				s1 += turn + ":" + Valuate(turn) + " ";
			}
			s1 += "} \\ " + best;
			//Debug.Log(s1);

			phase.Add (best);
			bag.Remove(best);

			List<Turn> newBag = new List<Turn>();
			for (int i = 0 ; i < bag.Count ; i++) {
				Turn t = bag[i];
				if(!Conflict(best,t)) {
					newBag.Add (t);
				}
			}
			bag = newBag;
		}

		//Update time values
		foreach(Turn t in allTurns) {
			if(phase.Contains(t)) {
				ResetTurnTimer(t);
			}
			else {
				IncrementTurnTimer(t);
			}
		}

		string s = name + " Phase: ";
		foreach(Turn t in phase) {
			s += t + ",";
		}
		//Debug.Log (s);

		return phase;
	}
	Turn FindMostValuableTurn(List<Turn> list) {
		Turn argmax = default(Turn);
		float valuemax = float.MinValue;
		foreach(Turn t in list) {
			float currentValue = Valuate (t);
			if(currentValue > valuemax) {
				argmax = t;
				valuemax = currentValue;
			}
		}
		return argmax;
	}
	float Valuate(Turn t) {
		Road inRoad = roads[t.in_idx];
		int queueSize = 0;
		foreach(Lane l in inRoad.InLanes(intersection)) {
			queueSize += l.GetCarsAtIntersection();
		}

		return queueSize + timeValueRatio * t.phasesSinceActivation;
	}
	bool Conflict(Turn t1, Turn t2) {
		if(t1.in_idx < t2.in_idx) {
			return t1.out_idx > t2.in_idx || (t2.out_idx < t2.in_idx && t1.in_idx < t2.out_idx);
		}
		else if(t1.in_idx > t2.in_idx) {
			return t2.out_idx > t1.in_idx || (t1.out_idx < t1.in_idx && t2.in_idx < t1.out_idx);
		}
		else {
			return false;
		}
	}

	void ResetTurnTimer(Turn t) {
		t.phasesSinceActivation = 0;
	}
	void IncrementTurnTimer(Turn t) {
		t.phasesSinceActivation++;
	}

	struct Turn {
		public int in_idx;
		public int out_idx;
		public int phasesSinceActivation;

		public Turn(int in_idx, int out_idx) {
			this.in_idx = in_idx;
			this.out_idx = out_idx;
			phasesSinceActivation = 0;
		}

		public override int GetHashCode () {
			return 31 * in_idx + out_idx;
		}

		public override bool Equals (object obj)
		{
			if(!obj.GetType().Equals(typeof(Turn))) {
				return false;
			}
			else {
				Turn t = (Turn)obj;
				return in_idx == t.in_idx && out_idx == t.out_idx;
			}
		}

		public override string ToString ()
		{
			return "(" + in_idx + "," + out_idx + ")";
		}
	}
}
