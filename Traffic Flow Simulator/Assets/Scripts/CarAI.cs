using UnityEngine;
using System.Collections.Generic;

public class CarAI {
	public static readonly float WAIT_MARGIN = 3;
	public static readonly float MINIMUM_DISTANCE_TO_NEXT_CAR = 7;
	public static readonly float TARGET_SECONDS_TO_NEXT_CAR = 2;
	public static readonly float SLOWDOWN_MARGIN = 2;
    public static readonly float ANGER_DECAY_INIT = 10;

	//Necessary fields
	Car car;
	public Route route;
	public int route_index;
	public ExplicitTurn nextTurn;

	//Personality
	public float baseline_anger;
    public float desired_speed_mod;
    public float anger_temper;

    //Emotion
    public float anger_state;
    public float anger_decay;
    public float honk_timer;

	//Evaluation methods
	public float EvaluateRoad(Road road) {
		return 1;
	}

	//Update behaviour
	public void Update() {
		if (route_index == -1)
		{
			RecomputeRoute();
			route_index = 0;
			Debug.Log ("Final Route: " + route);
			SelectNextTurn();
		}

		Intersection intersection = car.currentLane.to as Intersection;

		if(car.state == CarState.QUEUED) {
			car.speed = 0;
			if(car.nextCar == null) {
				bool allowedToDrive = intersection.IsOpen(nextTurn.Parent);
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

		else if(car.state == CarState.DRIVING ||
		        car.state == CarState.HEADING_TO_TURN ||
		        car.state == CarState.AWAY_FROM_TURN) {
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
				OnReachedIntersection(intersection);
				if(true || intersection.IsOpen(nextTurn.Parent)) {
					OnLightGreen(intersection);
				}
				else {
					OnLightRed(intersection);
				}
			}

			//Handle driving on intersection
			HandleStateChangeOnIntersection();

            //Get mad when driving below desired speed, depending on the amount below speed
            float desired_speed = car.currentLane.speedLimit * desired_speed_mod;
            if (car.speed < desired_speed /*&& car.state == CarState.DRIVING*/)
                anger_state += 0.01f * ((desired_speed - car.speed) / desired_speed) * anger_temper * Time.deltaTime;
            //Decaying of the anger depending on both the decay factor and temper factor
            anger_state -= 0.01f * (anger_decay / anger_temper) * Time.deltaTime;
            //Decaying of the decay of anger to imitate building irritability on longer drives
            anger_decay -= 0.01f * Time.deltaTime;
		}

        if (anger_state < 0)
            anger_state = 0;

        if (anger_state > 0.8f && car.nextCar != null)
        {
            if (honk_timer > 3.0f)
            {
                car.Honk();
                car.nextCar.ai.ReceiveHonk();
                honk_timer = 0;
            }
            else
                honk_timer += Time.deltaTime;
        }
        else if (honk_timer != 0)
            honk_timer = 0;
	}

	bool ReachedIntersection(Intersection intersection) {
		return intersection != null &&
			   car.currentLane.length - car.distanceOnLane < WAIT_MARGIN;
	}

	void OnReachedIntersection(Intersection intersection) {
		SelectNextTurn();
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

	void HandleStateChangeOnIntersection() {
		float testRadius = Time.deltaTime * car.speed;
		float distanceToEnd = Vector3.Distance(car.transform.position, nextTurn.LaneIn.endPoint);
		float distanceToTurn = Vector3.Distance(car.transform.position, nextTurn.TurnPoint);
		float distanceToStart = Vector3.Distance(car.transform.position, nextTurn.LaneOut.startPoint);

		if((car.state == CarState.DRIVING && distanceToEnd < testRadius)) {
			car.state = CarState.HEADING_TO_TURN;

			car.direction = (nextTurn.TurnPoint - car.transform.position).normalized;
		}
		if(car.state == CarState.HEADING_TO_TURN && distanceToTurn < testRadius) {
			car.state = CarState.AWAY_FROM_TURN;

			car.distanceOnLane = -distanceToStart;
			car.SwitchToLane(nextTurn.LaneOut);
			car.direction = (nextTurn.LaneOut.startPoint - car.transform.position).normalized;
		}
		if(car.state == CarState.AWAY_FROM_TURN && distanceToStart < testRadius) {
			car.state = CarState.DRIVING;

			car.distanceOnLane = 0;
			car.direction = nextTurn.LaneOut.direction;
			route_index++;
			SelectNextTurn();
		}
	}

	void Queue() {
		car.currentLane.Subscribe2Q(car);
		car.state = CarState.QUEUED;
	}
	void StartDriving() {
		car.currentLane.UnsubscribeFromQ(car);
	}

    public void ReceiveHonk()
    {
        anger_state += 0.01f * anger_temper;
    }

	//Pathfinding

	//Recomputes route of the agent
	//A queue of routes is kept. Then the first element is tested
	//If this route ends in the destination this is the new route
	//Otherwise all possible next turns are generated and the best lane chosen for new routes.
	//These new routes are pushed at the end of the queue.
	public void RecomputeRoute()
	{
		Queue<Route> routes = new Queue<Route>();
		Route start = new Route(car.currentLane.road, car.currentLane.to);
		routes.Enqueue(start);
		
		while (routes.Count > 0){
			Route route = routes.Dequeue();
			if (route.EndPoint == car.destination) {
				this.route = route;
				return;
			}
			else {
				ICollection<Route> nextRoutes = route.PossibleNextRoutes;
				if (nextRoutes != null) {
					foreach (Route nextRoute in nextRoutes) {
						routes.Enqueue(nextRoute);
					}
				}
			}
		}
		route_index = 0;
	}
	
	public void SelectNextTurn()
	{
		Intersection intersection = car.currentLane.to as Intersection;
		Road nextRoad = route[route_index + 1];

		//Find appropriate possible turn
		PossibleTurn possibleTurn = default(PossibleTurn);
		foreach (PossibleTurn turn in intersection.PossibleTurns)
		{
			if(turn.LaneIn == car.currentLane && turn.LanesOut[0].road == nextRoad) {
				possibleTurn = turn;
				break;
			}
		}

		//Find explicit turn with highest value
		ExplicitTurn argmax = default(ExplicitTurn);
		float value_max = float.MinValue;
		for (int i = 0 ; i < possibleTurn.LanesOut.Length ; i++) {
			Lane lane = possibleTurn.LanesOut[i];
			float value = EvaluateLane(lane);
			if(value > value_max) {
				argmax = new ExplicitTurn(possibleTurn,i);
				value_max = value;
			}
		}
		Debug.Log ("Next turn = " + argmax);
		nextTurn = argmax;
	}
	float EvaluateLane(Lane lane) {
		float value = 0;

		Road roadAfterNextTurn = route[route_index + 2];
		if(roadAfterNextTurn != null) {
			Road nextRoad = route[route_index + 1];
			Intersection upcomingIntersection = car.currentLane.to as Intersection;
			Intersection nextIntersection = nextRoad.OutLanes(upcomingIntersection)[0].to as Intersection;

			//Find road
			int idx = 0;
			Road[] roads = nextIntersection.roads;
			for (int i = 0 ; i < roads.Length ; i++) {
				if(nextRoad == roads[i]) {
					idx = i;
					break;
				}
			}
			int left_idx = (idx == 0) ? roads.Length - 1 : idx - 1;
			int right_idx = (idx == roads.Length - 1) ? 0 : idx + 1;
			if(roads[left_idx] == roadAfterNextTurn) {
				value += (lane.annotation == LaneAnnotation.LEFT) ? 1000 : 0;
			}
			else if(roads[right_idx] == roadAfterNextTurn) {
				value += (lane.annotation == LaneAnnotation.RIGHT) ? 1000 : 0;
			}
			else {
				value += (lane.annotation == LaneAnnotation.CENTER) ? 1000 : 0;
			}
		}
		float density = lane.CarsOnLane.Count / lane.length;
		value -= 10 * density;
		float proximity = Vector3.Distance(car.currentLane.endPoint, lane.startPoint);
		value -= proximity;

		return value;
	}

	public CarAI(Car car) {
		this.car = car;
        this.anger_state = UnityEngine.Random.Range(0,101) * 0.01f; //Needs to be set to base line anger
        this.desired_speed_mod = this.anger_state / 2 + 0.75f; //Depends on the base line anger
        this.anger_temper = UnityEngine.Random.Range(1, 11); //Needs to be given in the spawner
        this.anger_decay = ANGER_DECAY_INIT;
	}
}