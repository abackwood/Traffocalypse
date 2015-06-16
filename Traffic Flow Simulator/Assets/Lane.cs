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

	private List<Car> carsAtIntersection;
	private List<Car> carsOnLane;
	public Car[] CarsOnLane {
		get { return carsOnLane.ToArray(); }
	}

	/// <summary>
	/// Inializes the spatial components of the Lane object: length, direction and speed limit.
	/// </summary>
	/// <param name="speedLimit">Speed limit for the lane.</param>
	public void CalculateSpeedAndDirection(float speedLimit)
	{
		Vector3 difference = endPoint - startPoint;
		this.length = difference.magnitude;
		this.direction = difference.normalized;
		this.speedLimit = speedLimit;
	}

	/// <summary>
	/// Initializes the graphics component of the Lane object: the line renderer
	/// </summary>
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

	/// <summary>
	/// Subscribes a car to the queue at the end of the lane.
	/// </summary>
	/// <param name="car">Car.</param>
	public void Subscribe2Q(Car car)
	{
		carsAtIntersection.Add (car);
	}

	/// <summary>
	/// Unsubscribes a car from the waiting queue at the intersection.
	/// </summary>
	/// <param name="car">Car.</param>
	public void UnsubscribeFromQ(Car car)
	{
		carsAtIntersection.Remove (car);
	}

	void Start() {
		carsOnLane = new List<Car>();
		carsAtIntersection = new List<Car>();
	}

	void Update() {

	}
	
	public override string ToString ()
	{
		return road.name + "-" + id;
	}
}