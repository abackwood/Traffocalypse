﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarSpawner : MonoBehaviour {
	public Car template;
	public int carsToSpawn;
	public float cooldown;

	float timeSinceSpawn;
	List<Car> cars;

	// Use this for initialization
	void Start () {
		timeSinceSpawn = 0;
		cars = new List<Car>();
	}
	
	// Update is called once per frame
	void Update () {
		timeSinceSpawn += Time.deltaTime;
		if(cars.Count < carsToSpawn && timeSinceSpawn > cooldown) {
			SourceSink[] sources = SourceSink.Sources;
			SourceSink[] sinks = SourceSink.Sinks;

			Car car = GameObject.Instantiate(template);
			Lane lane;

			do {
				car.source = sources[UnityEngine.Random.Range(0,sources.Length)];
				car.destination = sinks[UnityEngine.Random.Range (0,sinks.Length)];
			} while(car.source == car.destination);

			CarAI ai = new CarAI(car);
			ai.baseline_anger = UnityEngine.Random.Range(0,1);
			car.ai = ai;

			Lane[] possibleLanes = car.source.road.OutLanes(car.source);
			lane = possibleLanes[0];
			foreach(Lane lane1 in possibleLanes) {
				if(ai.EvaluateLane(lane1,lane1) > ai.EvaluateLane(lane,lane)) {
					lane = lane1;
				}
			}
			if (lane.IsBlocked()) {
				GameObject.Destroy(car.gameObject);
				return;
			}

			car.currentLane = lane;
			car.currentLane.Subscribe(car);
			car.distanceOnLane = 0;
			car.transform.position = car.currentLane.startPoint;

			car.ReachedDestination += OnCarReachedDestination;

			cars.Add (car);

			timeSinceSpawn = 0;
		}
	}

	void OnCarReachedDestination(Car car) {
		cars.Remove (car);
		car.currentLane.Unsubcribe(car);
		GameObject.Destroy(car.gameObject);
	}
}