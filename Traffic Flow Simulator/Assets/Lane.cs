using UnityEngine;
using System.Collections.Generic;

[RequireComponent (typeof (LineRenderer))]
public class Lane : MonoBehaviour {
	public Road road;
	public int id;
	public Connection from;
	public Connection to;

	public float Length
	{
		get { return road.length; }
	}
	
	public float Speed
	{
		get { return road.speed; }
	}
	
	public float SpeedX
	{
		get { return road.SpeedX; }
	}
	
	public float SpeedY
	{
		get { return road.SpeedY; }
	}
	
	List<Car> carsOnLane;
	public Car[] CarsOnLane {
		get { return carsOnLane.ToArray(); }
	}
	
	public Lane(Road road, int id, Connection from, Connection to) {
		this.road = road;
		this.id = id;
		this.from = from;
		this.to = to;
		
		carsOnLane = new List<Car>();
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

	void Start() {

	}

	void Update() {

	}
	
	public override string ToString ()
	{
		return road.name + "-" + id + "(" + from.name + "-" + to.name + ")";
	}
}