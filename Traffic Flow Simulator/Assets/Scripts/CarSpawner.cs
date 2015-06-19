using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarSpawner : MonoBehaviour {
	public Car template;
	public int carsToSpawn;

	List<Car> cars;

	// Use this for initialization
	void Start () {
		cars = new List<Car>();
	}
	
	// Update is called once per frame
	void Update () {
		while(cars.Count < carsToSpawn) {
			SourceSink[] sources = SourceSink.Sources;
			SourceSink[] sinks = SourceSink.Sinks;

			Car car = GameObject.Instantiate(template);

			car.source = sources[UnityEngine.Random.Range(0,sources.Length)];
			car.destination = sinks[UnityEngine.Random.Range (0,sinks.Length)];

			Lane[] possibleLanes = car.source.road.OutLanes(car.source);
			car.currentLane = possibleLanes[UnityEngine.Random.Range(0,possibleLanes.Length)];
			car.distanceOnLane = 0;
			car.transform.position = car.currentLane.startPoint;

			car.ReachedDestination += OnCarReachedDestination;

			cars.Add (car);
		}
	}

	void OnCarReachedDestination(Car car) {
		cars.Remove (car);
		GameObject.Destroy(car);
	}
}
