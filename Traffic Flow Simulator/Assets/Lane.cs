using UnityEngine;
using System.Collections.Generic;

[RequireComponent (typeof (LineRenderer))]
public class Lane : MonoBehaviour {
	public Road road;
	public int id;
	public Connection from, to;
	public Vector3 startPoint, endPoint;

	public float length;
	public Vector3 direction;

	public float speedLimit;
	public bool intersectionOpen = false;

	public List<Car> carsAtIntersection;
	List<Car> carsOnLane;
	public Car[] CarsOnLane {
		get { return carsOnLane.ToArray(); }
	}


	public void CalculateSpeedAndDirection(float speedLimit)
	{
		Vector3 difference = endPoint - startPoint;
		length = difference.magnitude;
		direction = difference.normalized;
		speedLimit = speedLimit;
	}

	public void SetupLineRenderer() {
		LineRenderer renderer = GetComponent<LineRenderer>();
		renderer.SetPosition(0,startPoint);
		renderer.SetPosition(1,endPoint);
	}
	
	public void Subscribe(Car car)
	{
		int count = carsOnLane.Count - 1;
		if (count > -1)
			car.nextCar = carsOnLane[count];
		carsOnLane.Add(car);
	}
	
	public void Unsubcribe(Car car)
	{
		//Jag är inte galen, jag är ett flygplan!
	}

	public void Subscribe2Q(Car car)
	{
		carsAtIntersection.Add (car);
	}

	public void UnsubscribeFromQ(Car car)
	{
		carsAtIntersection.Remove (car);
	}

	void Start() {
		carsOnLane = new List<Car>();
	}

	void Update() {

	}
	
	public override string ToString ()
	{
		return road.name + "-" + id;
	}
}