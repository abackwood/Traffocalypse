using UnityEngine;
using System.Collections.Generic;

public struct Route {
	List<Connection> intersectionsCrossed;

	Road[] route;
	public Road this[int i] {
		get { return i < route.Length ? route[i] : null; }
	}

	public Road Last {
		get { return route[route.Length - 1]; }
	}

	Connection endPoint;
	public Connection EndPoint {
		get { return endPoint; }
	}

	//Calculates all possible turns that could be made from this route
	//If the current end point is a source/sink or an intersection that has
	//already been visited, this returns null
	public ICollection<Route> PossibleNextRoutes {
		get {
			Intersection intersection = EndPoint as Intersection;
			if(intersectionsCrossed.Contains(EndPoint) ||
			   intersection == null) {
				return null;
			}
			else {
				List<Route> list = new List<Route>();
				foreach(Road road in intersection.roads) {
					if(road != Last && road.OutLanes(intersection).Length > 0) {
						Connection newEndPoint = road.OutLanes(intersection)[0].to;
						Route newRoute = new Route(this,road,newEndPoint);
						list.Add (newRoute);
					}
				}
				return list;
			}
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Route"/> struct.
	/// </summary>
	/// <param name="road">Road.</param>
	/// <param name="endPoint">End point connected to the road that is the direction of travel.</param>
	public Route(Road road, Connection endPoint) {
		intersectionsCrossed = new List<Connection>();

		this.endPoint = endPoint;
		route = new Road[]{road};
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Route"/> struct.
	/// </summary>
	/// <param name="oldRoute">The old route that's being added on to.</param>
	/// <param name="turn">Turn.</param>
	public Route(Route oldRoute, Road road, Connection endPoint) {
		intersectionsCrossed = new List<Connection>(oldRoute.intersectionsCrossed);
		intersectionsCrossed.Add (oldRoute.endPoint);

		this.endPoint = endPoint;
		this.route = new Road[oldRoute.route.Length + 1];
		for(int i = 0 ; i < oldRoute.route.Length ; i++) {
			this.route[i] = oldRoute[i];
		}
		route[route.Length - 1] = road;
	}

	public override string ToString ()
	{
		string s = "-> ";
		for(int i = 0 ; i < route.Length ; i++) {
			s += route[i] + " -> ";
		}
		return s;
	}
}