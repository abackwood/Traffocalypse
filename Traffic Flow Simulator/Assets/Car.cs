using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Car : MonoBehaviour {
	public delegate void CarEventHandler(Car car);

	public event CarEventHandler Spawned, ReachedDestination;

	public Lane currentLane;
	public float position;
	public SourceSink source, destination;
	public float distanceOnLane;
	public Car nextCar;
	public bool waitIntersection = false;
	public bool onIntersection = false;

	//TODO make extendable states
	public CarState state;

	Route route;
	int route_index;

	CarAI ai;

    public float[] speeds = new float[7] {0, 0.2f, 0.4f, 0.6f, 0.8f, 1.0f, 1.2f};
    public int currentSpeed = 0;

	// Use this for initialization
	void Start () {
		ai = new SimpleCarAI();
		route_index = -1;

		if(Spawned != null) {
			Spawned(this);
		}
	}
	
	// Update is called once per frame
	void Update () {
		//Some sort of pathfinding
        //if(route_index == -1) {
        //    RecomputeRoute ();
        //    Debug.Log ("Final Route: " + route);
        //}

        //if(IsAtDestination() && ReachedDestination != null) {
        //    ReachedDestination(this);
        //}

		//Driving
        CheckSpeed();
		Move();
		//	Collision prevention
		//	Set speed
		//	Move down lane
		//	Turn at intersections

		/*
		 * if QUEUED
		 * 		if FRONT CAR
		 * 			if possible turn green, then state DRIVING
		 * 		else
		 * 			if next car is DRIVING, then state DRIVING
		 * if DRIVING
		 * 		if at intersection OR next car is QUEUED and right behind it
		 * 			then state QUEUED
		 * 		else
		 * 			keep safe distance, drive at desired speed, etc.
		 */
				
	}

	bool IsAtDestination() {
		return Vector3.Distance(transform.position, destination.transform.position) < 0.1f;
	}

	void StartNewLane()
	{
	}

    void CheckSpeed()
    {
        // Calculate break time and free distance on road (currently doesn't take other cars into acount)
        float leftOnLane = currentLane.length - distanceOnLane;
        float beforeStop = 0;
        int tempCurrentSpeed = currentSpeed;
        while (tempCurrentSpeed != 0)
        {
            tempCurrentSpeed -= 1;
            beforeStop += speeds[tempCurrentSpeed];
        }

        // Check if we need to hit the break
        if ((beforeStop + speeds[currentSpeed]) >= leftOnLane)
        {
            if (currentSpeed != 0)
                currentSpeed--;
        }

        // Check if we can accerlate
        else if (currentSpeed < speeds.Length - 1)
        {
            if (speeds[currentSpeed + 1] <= currentLane.speedLimit)
                currentSpeed++;
        }

        // Debug info
        Debug.Log(currentSpeed + " " + speeds[currentSpeed] + " " + distanceOnLane + " " + beforeStop);
    }

	void Move()
	{
		Intersection intersection = currentLane.to as Intersection;

		// First check whether we reached the intersection
		if (intersection != null && distanceOnLane > currentLane.length - 5)
		{
			// If the traffic light is green, go ahead
			if(currentLane.intersectionOpen)
			{
				currentLane.UnsubscribeFromQ(this);

				foreach(PossibleTurn turn in intersection.PossibleTurns) {
					if(turn.LaneIn == currentLane) {
						currentLane = turn.LanesOut[0];
					}
				}
				
				transform.position = new Vector3(currentLane.startPoint.x, currentLane.startPoint.y, 0);
				onIntersection = false;
				distanceOnLane = 0;

				return;
			}
			else
			{
				if(!onIntersection)
				{
					currentLane.Subscribe2Q(this);
					onIntersection = true;
				}
				return;
			}
		}

		if (nextCar != null && nextCar.distanceOnLane - distanceOnLane < 200 * currentLane.speedLimit)
		{
			if (nextCar.onIntersection && !onIntersection)
			{
				onIntersection = true;
				currentLane.Subscribe2Q(this);
			}
			return;
		}

        distanceOnLane += speeds[currentSpeed];
		
		transform.Translate(currentLane.direction * speeds[currentSpeed]);
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

public enum CarState {
	DRIVING,
	QUEUED,
	BROKEN
}