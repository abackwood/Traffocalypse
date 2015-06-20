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

	private List<Car> carsAtIntersection;
	private LinkedList<Car> carsOnLane;
	public ICollection<Car> CarsOnLane {
		get { return carsOnLane; }
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

	/// <summary>
	/// Subscribes the specified car to the back of the lane, assumed to be behind every other car already there
	/// </summary>
	/// <param name="car">The subscribing car.</param>
	public void Subscribe(Car car)
	{
		if(carsOnLane.Count > 0) {
			car.nextCar = carsOnLane.Last.Value;
		}
		carsOnLane.AddLast(car);
	}

	/// <summary>
	/// Unsubscribes a car from the lane. The information about next cars is updated for the other cars still on the lane
	/// </summary>
	/// <param name="car">The car that's leaving.</param>
	public void Unsubcribe(Car car)
	{
		LinkedListNode<Car> node = carsOnLane.Find (car);
		if(node != null) {
			if(node.Next != null) {
				node.Next.Value.nextCar = node.Previous == null ? null : node.Previous.Value;
			}
			carsOnLane.Remove(node);
		}
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
		carsOnLane = new LinkedList<Car>();
		carsAtIntersection = new List<Car>();
	}
	
	public override string ToString ()
	{
		return road.name + "-" + id;
	}
}