using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Car : MonoBehaviour {
	public delegate void CarEventHandler(Car car);

	public event CarEventHandler Spawned, ReachedDestination;

	public Lane currentLane;
	public SourceSink source, destination;
	public float distanceOnLane;
	public Car nextCar;

	public CarState state;
	public float speed;

	Route route;
	int route_index;
	public ExplicitTurn NextTurn {
		get { return route[route_index]; }
	}

	CarAI ai;

	// Use this for initialization
	void Start () {
		ai = new CarAI(this);
		route_index = -1;

		currentLane.Subscribe(this);

		if(Spawned != null) {
			Spawned(this);
		}
	}
	
	// Update is called once per frame
	void Update () {
		//Some sort of pathfinding
		if(route_index == -1) {
			RecomputeRoute ();
			Debug.Log ("Final Route: " + route);
		}

		if(IsAtDestination() && ReachedDestination != null) {
			ReachedDestination(this);
		}

		ai.Update();
		Move();
		//	Collision prevention
		//	Set speed
		//	Move down lane
		//	Turn at intersections
	}

	bool IsAtDestination() {
		return currentLane.to == destination && currentLane.length - distanceOnLane <= 0;
	}

	void StartNewLane()
	{
	}
	
	void Move()
	{
		float trueSpeed = speed * Time.deltaTime;
		Vector3 movement = currentLane.direction * trueSpeed;

		Intersection intersection = currentLane.to as Intersection;

		if(intersection != null) {
			float distanceToTurn = Vector3.Distance(transform.position, NextTurn.TurnPoint);
			
			//If a turn point will be reached special actions need to be taken
			if(movement.magnitude > distanceToTurn) {
				Lane newLane = NextTurn.LaneOut;	//Find new lane
				float newDistanceOnLane = Vector3.Distance(NextTurn.TurnPoint, newLane.startPoint);
				
				//Move position and adjust car location internally
				transform.position = NextTurn.TurnPoint;	//Drive to next turn
				SwitchToLane(newLane);
				distanceOnLane = -newDistanceOnLane;		//You start somewhat behind the start of the lane

				route_index++;

				//Recalculate speed and movement
				trueSpeed = movement.magnitude - distanceToTurn;	//Speed is now the rest after driving to the turn
				movement = currentLane.direction * trueSpeed;	//Recalculate movement
			}
		}

		distanceOnLane += trueSpeed;
		transform.Translate(movement);
	}
	void SwitchToLane(Lane lane) {
		currentLane.Unsubcribe(this);
		currentLane = lane;
		currentLane.Subscribe(this);
	}

	//Recomputes route of the agent
	//A queue of routes is kept. Then the first element is tested
	//If this route ends in the destination this is the new route
	//Otherwise all possible next turns are generated and the best lane chosen for new routes.
	//These new routes are pushed at the end of the queue.
	public void RecomputeRoute() {
		Connection nextNode = currentLane.to;
		Queue<Route> routes = new Queue<Route>(GenerateRoutesFromNextNode(nextNode));

		while(routes.Count > 0) {
			Route route = routes.Dequeue ();
			if (route.EndPoint == destination) {
				this.route = route;
				route_index = 0;
				return;
			}
			else {
				ICollection<PossibleTurn> possibleNextTurns = route.PossibleNextTurns;
				if(possibleNextTurns == null) {
					continue;
				}
				foreach(PossibleTurn turn in possibleNextTurns) {
					ExplicitTurn bestTurn = SelectBestExplicitTurn(turn);
					Route new_route = new Route(route,bestTurn,ai);
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
				if(turn.LaneIn == currentLane) {
					ExplicitTurn bestTurn = SelectBestExplicitTurn(turn);
					Route new_route = new Route(bestTurn,ai);
					routes.Add (new_route);
				}
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

public enum CarState {
	DRIVING,
	QUEUED,
	ON_INTERSECTION
}
