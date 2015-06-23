
 using UnityEngine;
using System.Collections.Generic;

public class CarAI {
	public static readonly float WAIT_MARGIN = 5;
	public static readonly float MINIMUM_DISTANCE_TO_NEXT_CAR = 5;
	public static readonly float TARGET_SECONDS_TO_NEXT_CAR = 2;
	public static readonly float SLOWDOWN_MARGIN = 2;
    public static readonly float ANGER_DECAY_INIT = 10;

	//Necessary fields
	Car car;
	public Route route;
	public int route_index;
	public ExplicitTurn NextTurn
	{
		get { return route[route_index]; }
	}

	//Personality
	public float baseline_anger;
    public float desired_speed_mod;
    public float anger_temper;

    //Emotion
    public float anger_state;
    public float anger_decay;

	//Evaluation methods
	public float EvaluateRoad(Road road) {
		return 1;
	}

	public float EvaluateLane(Lane currentLane, Lane lane) {
		float density = lane.CarsOnLane.Count / lane.length;
		float density_score = -density;
		float closeness_score = -Vector3.Distance(currentLane.endPoint, lane.startPoint);

		return 10 * density_score + closeness_score;
	}

	//Update behaviour
	public void Update() {
		//Reclacualte route if necesssary
		if (route_index == -1)
		{
			RecomputeRoute();
			//Debug.Log ("Final Route: " + route);
		}

		Intersection intersection = car.currentLane.to as Intersection;

		if(car.state == CarState.QUEUED) {
			car.speed = 0;
			if(car.nextCar == null) {
				bool allowedToDrive = intersection.IsOpen(NextTurn.Parent);
				if(allowedToDrive) {
					OnLightTurnedGreen(intersection);
				}
			}
			else {
				if(car.nextCar.distanceOnLane - car.distanceOnLane > MINIMUM_DISTANCE_TO_NEXT_CAR) {
					car.speed = 0.5f * car.currentLane.speedLimit;
				}
				if(car.nextCar.state == CarState.DRIVING) {
					OnCarInFrontStartedDriving();
				}
			}
		}

		else if(car.state == CarState.DRIVING || car.state == CarState.ON_INTERSECTION) {
            //Speed changes depending on anger state from 0.75x to 1.5x the speedlimit
            car.speed = car.currentLane.speedLimit * (anger_state * 0.75f + 0.75f);

			//If behind another car, avoid collision by matching speed and queue up behind them if applicable
			if(car.nextCar != null) {
				float distanceToNextCar = (car.nextCar.distanceOnLane - car.distanceOnLane);
				KeepDistanceFromNextCar(distanceToNextCar);
			}

			//If you are the front car and you've reached the intersection, respond to the red or green light
			else if(car.state == CarState.DRIVING &&
			        ReachedIntersection(intersection)) {

				if(intersection.IsOpen(NextTurn.Parent)) {
					OnLightGreen(intersection);
				}
				else {
					OnLightRed(intersection);
				}
			}

			//Detect when no longer on intersection
			if(car.state == CarState.ON_INTERSECTION &&
			   car.distanceOnLane > 0 && car.distanceOnLane < car.currentLane.length) {
				car.state = CarState.DRIVING;
			}

            float desired_speed = car.currentLane.speedLimit * desired_speed_mod;
            //Get mad when driving below desired speed, depending on the amount below speed
            if (car.speed < desired_speed /*&& car.state == CarState.DRIVING*/)
                anger_state += 0.0001f * ((desired_speed - car.speed) / desired_speed) * anger_temper;
		}

        //Decaying of the anger depending on both the decay factor and temper factor
        anger_state -= 0.0001f * (anger_decay / anger_temper);
        //Decaying of the decay of anger to imitate building irritability on longer drives
        anger_decay -= 0.01f;
	}

	bool ReachedIntersection(Intersection intersection) {
		return intersection != null &&
			   car.currentLane.length - car.distanceOnLane < WAIT_MARGIN;
	}

	void OnLightRed(Intersection intersection) {
		Queue();
	}

	void OnLightGreen(Intersection intersection) {
		StartDriving();
	}

	void OnLightTurnedGreen(Intersection intersection) {
		StartDriving();
	}

	void OnCarInFrontStartedDriving() {
		car.state = CarState.DRIVING;
	}

	void KeepDistanceFromNextCar(float distance) {
		float targetDistance = Mathf.Max(MINIMUM_DISTANCE_TO_NEXT_CAR, car.speed * TARGET_SECONDS_TO_NEXT_CAR);

        //If more angry than 50% target distance to next car will shrink up to half its original size
        if (anger_state > 0.5f) {
			targetDistance = targetDistance / (2.0f * anger_state);
		}

		if(distance < targetDistance) {
			car.speed = Mathf.Max(0, car.nextCar.speed - SLOWDOWN_MARGIN);
			if(car.nextCar.state == CarState.QUEUED) {
				Queue();
			}
		}
	}

	void Queue() {
		car.currentLane.Subscribe2Q(car);
		car.state = CarState.QUEUED;
	}
	void StartDriving() {
		car.currentLane.UnsubscribeFromQ(car);
		car.state = CarState.ON_INTERSECTION;
	}

	//Pathfinding

	//Recomputes route of the agent
	//A queue of routes is kept. Then the first element is tested
	//If this route ends in the destination this is the new route
	//Otherwise all possible next turns are generated and the best lane chosen for new routes.
	//These new routes are pushed at the end of the queue.
	public void RecomputeRoute()
	{
		Connection nextNode = car.currentLane.to;
		Queue<Route> routes = new Queue<Route>(GenerateRoutesFromNextNode(nextNode));
		
		while (routes.Count > 0)
		{
			Route route = routes.Dequeue();
			if (route.EndPoint == car.destination)
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
					Route new_route = new Route(route, bestTurn, this);
					routes.Enqueue(new_route);
				}
			}
		}
		route_index = 0;
	}
	
	ICollection<Route> GenerateRoutesFromNextNode(Connection nextNode)
	{
		if (nextNode.GetType().Equals(typeof(SourceSink)))
		{
			return null;
		}
		else
		{
			List<Route> routes = new List<Route>();
			Intersection intersection = (Intersection)nextNode;
			foreach (PossibleTurn turn in intersection.PossibleTurns)
			{
				if (turn.LaneIn == car.currentLane)
				{
					ExplicitTurn bestTurn = SelectBestExplicitTurn(turn);
					Route new_route = new Route(bestTurn, this);
					routes.Add(new_route);
				}
			}
			return routes;
		}
	}
	
	ExplicitTurn SelectBestExplicitTurn(PossibleTurn turn)
	{
		ExplicitTurn argmax = default(ExplicitTurn);
		float value_max = float.MinValue;
		
		for (int i = 0; i < turn.LanesOut.Length; i++)
		{
			float value = EvaluateLane(turn.LaneIn, turn.LanesOut[i]);
			if (value > value_max)
			{
				argmax = new ExplicitTurn(turn, i);
				value_max = value;
			}
		}
		return argmax;
	}

	public CarAI(Car car) {
		this.car = car;
        this.anger_state = UnityEngine.Random.Range(0,101) * 0.01f; //Needs to be set to base line anger
        this.desired_speed_mod = this.anger_state / 2 + 0.75f; //Depends on the base line anger
        this.anger_temper = UnityEngine.Random.Range(1, 11); //Needs to be given in the spawner
        this.anger_decay = ANGER_DECAY_INIT;
	}
}