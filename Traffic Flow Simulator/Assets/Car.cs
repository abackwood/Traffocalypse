using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Car : MonoBehaviour {
	public Lane currentLane;
	public float position;
	public SourceSink source, destination;
	public float distanceOnLane;
	public Car nextCar;
    public bool waitIntersection = false;
    public bool onIntersection = false;

	Route route;
	int route_index;

	CarAI ai;

	// Use this for initialization
	void Start () {
		Lane[] possibleLanes = source.road.OutLanes(source);	//All lanes going out from the source
		currentLane = possibleLanes[0];		//Arbitrary choice of lane, might want to choose randomly or with a heuristic at some point

		ai = new SimpleCarAI();
		route_index = -1;
	}
	
	// Update is called once per frame
	void Update () {
		//Some sort of pathfinding
        //if(route_index == -1) {
        //    RecomputeRoute ();
        //    Debug.Log ("Final Route: " + route);
        //}

		//Driving
		//	Collision prevention
		//	Set speed
		//	Move down lane
		//	Turn at intersections

        Move();
	}

	void StartNewLane()
	{
	}
	
	void Move()
	{
        
		if (nextCar != null && nextCar.distanceOnLane - distanceOnLane < 200 * currentLane.Speed)
		{
			return;
		}

        Intersection intersection = currentLane.To as Intersection;
        if(onIntersection)
        {
            currentLane = intersection.roads[1].LanesForward[0];
            transform.position = new Vector3(currentLane.FromPosition.x, currentLane.FromPosition.y, 0);
            distanceOnLane = 0;
            onIntersection = false;
            return;
        }

		
		distanceOnLane += currentLane.Speed;

		if (intersection != null && distanceOnLane > currentLane.Length - 5)
		{
            waitIntersection = true;
            System.Threading.Thread.Sleep(1000);
            waitIntersection = false;
            onIntersection = true;
            return;
		}
        else if(distanceOnLane > currentLane.length)
        {
            transform.position = new Vector3(currentLane.ToPosition.x, currentLane.ToPosition.y, 0);
            distanceOnLane -= currentLane.Speed;
            return;
        }

		transform.Translate(new Vector3(currentLane.SpeedX, currentLane.SpeedY, 0));
	}

	//Recomputes route of the agent
	//A queue of routes is kept. Then the first element is tested
	//If this route ends in the destination this is the new route
	//Otherwise all possible next turns are generated and the best lane chosen for new routes.
	//These new routes are pushed at the end of the queue.
    public void RecomputeRoute()
    {
        Connection nextNode = currentLane.To;
        Queue<Route> routes = new Queue<Route>(GenerateRoutesFromNextNode(nextNode));

        while (routes.Count > 0)
        {
            Route route = routes.Dequeue();
            Debug.Log(route);
            if (route.EndPoint == destination)
            {
                this.route = route;
                route_index = 0;
                return;
            }
            else
            {
                ICollection<PossibleTurn> possibleNextTurns = route.PossibleNextTurns;
                if (possibleNextTurns == null)
                {
                    continue;
                }
                foreach (PossibleTurn turn in possibleNextTurns)
                {
                    ExplicitTurn bestTurn = SelectBestExplicitTurn(turn);
                    Route new_route = new Route(route, bestTurn, ai);
                    routes.Enqueue(new_route);
                }
            }
        }
        route_index = 0;
    }

	ICollection<Route> GenerateRoutesFromNextNode(Connection nextNode) {
		if(nextNode.GetType().Equals (typeof(SourceSink))) {
			return null;
		}
		else {
			List<Route> routes = new List<Route>();
			Intersection intersection = (Intersection)nextNode;
			foreach(PossibleTurn turn in intersection.PossibleTurns) {
				ExplicitTurn bestTurn = SelectBestExplicitTurn(turn);
				Route new_route = new Route(bestTurn,ai);
				routes.Add (new_route);
			}
			return routes;
		}
	}

	ExplicitTurn SelectBestExplicitTurn(PossibleTurn turn) {
		ExplicitTurn argmax = default(ExplicitTurn);
		float value_max = float.MinValue;

		for(int i = 0 ; i < turn.LanesOut.Length ; i++) {
			float value = ai.EvaluateLane(turn.LanesOut[i]);
			if(value > value_max) {
				argmax = new ExplicitTurn(turn,i);
				value_max = value;
			}
		}
		return argmax;
	}
}
