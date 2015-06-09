using UnityEngine;
using System.Collections.Generic;

public struct Route {
	List<Connection> intersectionsCrossed;

	ExplicitTurn[] route;
	public ExplicitTurn this[int i] {
		get { return route[i]; }
	}

	public ExplicitTurn Last {
		get { return route[route.Length - 1]; }
	}

	public Connection EndPoint {
		get { return Last.LaneOut.To; }
	}

	//Calculates all possible turns that could be made from this route
	//If the current end point is a source/sink or an intersection that has
	//already been visited, this returns null
	public ICollection<PossibleTurn> PossibleNextTurns {
		get {
			if(intersectionsCrossed.Contains(EndPoint) ||
			   EndPoint.GetType().Equals (typeof(SourceSink))) {
				return null;
			}
			else {
				Intersection intersection = (Intersection)EndPoint;
				List<PossibleTurn> list = new List<PossibleTurn>();
				foreach(PossibleTurn turn in intersection.PossibleTurns) {
					if(turn.LaneIn.Equals(Last.LaneOut)) {
						list.Add (turn);
					}
				}
				return list;
			}
		}
	}

	//The value of the route, which is the sum of road values it implicitly contains
	float routeValue;
	public float RouteValue {
		get { return routeValue; }
	}

	//Initializes a route with only one turn, forming the starting point for
	//pathfinding. The value is that of the road assumed to be the current location
	public Route(ExplicitTurn turn, CarAI ai) {
		intersectionsCrossed = new List<Connection>();
		intersectionsCrossed.Add (turn.Intersection);

		route = new ExplicitTurn[]{turn};
		routeValue = ai.EvaluateRoad(turn.LaneIn.Road);
	}

	//Initializes a route based on a previous route, with an added explicit turn
	//Route value is that of the old route plus the value of the added road
	public Route(Route oldRoute, ExplicitTurn turn, CarAI ai) {
		intersectionsCrossed = new List<Connection>(oldRoute.intersectionsCrossed);
		intersectionsCrossed.Add (turn.Intersection);

		route = new ExplicitTurn[oldRoute.route.Length + 1];
		for(int i = 0 ; i < oldRoute.route.Length ; i++) {
			route[i] = oldRoute[i];
		}
		route[route.Length - 1] = turn;

		Road newRoad = oldRoute.Last.LaneOut.Road;
		routeValue = oldRoute.routeValue + ai.EvaluateRoad(newRoad);
	}

	public override string ToString ()
	{
        //string s = "-> ";
        //for(int i = 0 ; i < route.Length ; i++) {
        //    s += route[i] + " -> ";
        //}
        //return s;
        return "";
	}
}