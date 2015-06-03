using UnityEngine;
using System.Collections.Generic;

[RequireComponent (typeof (LineRenderer))]
public class Lane : MonoBehaviour {
	Road road;
	public Road Road {
		get { return road; }
	}
	
	int id;
	public int ID {
		get { return id; }
	}
	
	Connection from;
	public Connection From {
		get { return from; }
	}
	
	Connection to;
	public Connection To {
		get { return to; }
	}
	
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

    public void Initiate(Road road, int id, Connection from, Connection to)
    {
        this.road = road;
        this.id = id;
        this.from = from;
        this.to = to;

        carsOnLane = new List<Car>();

        float xDifference = from.transform.position.x - to.transform.position.x;
        float yDifference = from.transform.position.y - to.transform.position.y;
        float length = Mathf.Sqrt((xDifference * xDifference) + (yDifference * yDifference));
        float xNorm = xDifference / length * 5;
        float yNorm = yDifference / length * 5;

        Vector3 fromPos = from.transform.position;
        fromPos.y += xNorm;
        fromPos.x -= yNorm;
        Vector3 toPos = to.transform.position;
        toPos.y += xNorm;
        toPos.x -= yNorm;

        LineRenderer renderer = GetComponent<LineRenderer>();

        float width = 1f;
        renderer.SetWidth(width, width);

        renderer.SetPosition(0, fromPos);
        renderer.SetPosition(1, toPos);
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

	void Start() 
    {

	}

	void Update() {

	}
	
	public override string ToString ()
	{
		return Road.name + "-" + id + "(" + from.name + "-" + to.name + ")";
	}
}