using UnityEngine;

public interface CarAI {
	float EvaluateRoad(Road road);
	float EvaluateLane(Lane lane);
}

public class SimpleCarAI : CarAI {
	public float EvaluateRoad(Road road) {
		return 0;
	}

	public float EvaluateLane(Lane lane) {
		return lane.ID;
	}
}