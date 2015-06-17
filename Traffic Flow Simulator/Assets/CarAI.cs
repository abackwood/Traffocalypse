using UnityEngine;

public class CarAI {
	public static readonly float WAIT_MARGIN = 5;
	public static readonly float MINIMUM_DISTANCE_TO_NEXT_CAR = 5;
	public static readonly float TARGET_SECONDS_TO_NEXT_CAR = 1;
	public static readonly float SLOWDOWN_MARGIN = 1;

	Car car;

	public float EvaluateRoad(Road road) {
		return 1;
	}

	public float EvaluateLane(Lane lane) {
		return lane.id;
	}

	public void Update() {
		Intersection intersection = car.currentLane.to as Intersection;

		if(car.state == CarState.QUEUED) {
			car.speed = 0;
			if(car.nextCar == null) {
				bool allowedToDrive = intersection.IsOpen(car.NextTurn.Parent);
				if(allowedToDrive) {
					StartDriving();
				}
			}
			else {
				if(car.nextCar.state == CarState.DRIVING) {
					car.state = CarState.DRIVING;
				}
			}
		}

		else if(car.state == CarState.DRIVING || car.state == CarState.ON_INTERSECTION) {
			car.speed = car.currentLane.speedLimit;

			//If behind another car, avoid collision by matching speed and queue up behind them if applicable
			if(car.nextCar != null) {
				Car nextCar = car.nextCar;

				float distanceToNextCar = (nextCar.distanceOnLane - car.distanceOnLane);
				float targetDistance = Mathf.Max(MINIMUM_DISTANCE_TO_NEXT_CAR, car.speed * TARGET_SECONDS_TO_NEXT_CAR);
				if(distanceToNextCar < targetDistance) {
					car.speed = car.nextCar.speed - SLOWDOWN_MARGIN;
					if(nextCar.state == CarState.QUEUED) {
						Queue();
					}
				}
			}

			//If you are the front car and you've reached the intersection, queue for the intersection
			else if(car.state == CarState.DRIVING &&
			        ReachedIntersection(intersection)) {
				Queue();
			}

			//Detect when no longer on intersection
			if(car.state == CarState.ON_INTERSECTION &&
			   car.distanceOnLane > 0 && car.distanceOnLane < car.currentLane.length) {
				car.state = CarState.DRIVING;
			}
		}
	}

	bool ReachedIntersection(Intersection intersection) {
		return intersection != null &&
				car.currentLane.length - car.distanceOnLane < WAIT_MARGIN &&
				!intersection.IsOpen(car.NextTurn.Parent);
	}

	void Queue() {
		car.currentLane.Subscribe2Q(car);
		car.state = CarState.QUEUED;
	}
	void StartDriving() {
		car.currentLane.UnsubscribeFromQ(car);
		car.state = CarState.ON_INTERSECTION;
	}

	public CarAI(Car car) {
		this.car = car;
	}
}