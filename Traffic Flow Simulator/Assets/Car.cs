using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Car : MonoBehaviour {
	public Lane currentLane;
	public float position;
	public SourceSink source, destination;
	public float distanceOnLane;
	public Car nextCar;

	CarAI ai;

	// Use this for initialization
	void Start () {
		Lane[] possibleLanes = source.road.OutLanes(source);	//All lanes going out from the source
		currentLane = possibleLanes[0];		//Arbitrary choice of lane, might want to choose randomly or with a heuristic at some point
	}
	
	// Update is called once per frame
	void Update () {
		//Some sort of pathfinding
		//Driving
		//	Collision prevention
		//	Set speed
		//	Move down lane
		//	Turn at intersections
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
		
		distanceOnLane += currentLane.Speed;
		
		if (distanceOnLane > currentLane.Length)
		{
			transform.position = new Vector3(currentLane.To.transform.position.x, currentLane.To.transform.position.y, 0);
			distanceOnLane -= currentLane.Speed;
			return;
		}
		
		transform.Translate(new Vector3(currentLane.SpeedX, currentLane.SpeedY, 0));
	}

	void RecomputeRoute() {
		Connection nextNode = currentLane.To;


	}

	Road SelectRoadWithMaxValue(Road[] choices) {
		Road argmax = null;
		float max_value = float.MinValue;
		foreach(Road road in choices) {
			float road_value = ai.EvaluateRoad(road);
			if(road_value > max_value) {
				argmax = road;
				max_value = road_value;
			}
		}
		return argmax;
	}
}
