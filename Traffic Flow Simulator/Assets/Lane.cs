using UnityEngine;
using System.Collections.Generic;

[RequireComponent (typeof (LineRenderer))]
public class Lane : MonoBehaviour {

    public Transform car;

    public float length;
    public float speed;
    public float xSpeed;
    public float ySpeed;

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
		get { return length; }
	}
	
	public float Speed
	{
		get { return speed; }
	}
	
	public float SpeedX
	{
		get { return xSpeed; }
	}
	
	public float SpeedY
	{
		get { return ySpeed; }
	}

    Vector3 fromPos;
    public Vector3 FromPosition
    {
        get { return fromPos; }
    }

    Vector3 toPos;
    public Vector3 ToPosition
    {
        get { return toPos; }
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

        fromPos = from.transform.position;
        fromPos.y += xNorm;
        fromPos.x -= yNorm;
        toPos = to.transform.position;
        toPos.y += xNorm;
        toPos.x -= yNorm;

        LineRenderer renderer = GetComponent<LineRenderer>();

        float width = 1f;
        renderer.SetWidth(width, width);

        renderer.SetPosition(0, fromPos);
        renderer.SetPosition(1, toPos);

        CalculateSpeed();

        Transform currentCar = Instantiate(car, from.transform.position, Quaternion.identity) as Transform;
        Car carScript = currentCar.GetComponent<Car>();
        carScript.source = from as SourceSink;
        carScript.destination = to as SourceSink;
        currentCar.position = fromPos;
        currentCar.localScale = new Vector3(25, 25, 0);
    }

    void CalculateSpeed()
    {
        speed = 0.5f;
        float xDifference = Mathf.Abs(fromPos.x - toPos.x);
        float yDifference = Mathf.Abs(fromPos.y - toPos.y);
        length = Mathf.Sqrt((xDifference * xDifference) + (yDifference * yDifference));
        if(fromPos.x < toPos.x)
            xSpeed = (xDifference * speed) / length;
        else
            xSpeed = (xDifference * speed) / length * -1;
        if (fromPos.y < toPos.y)
            ySpeed = (yDifference * speed) / length;
        else
            ySpeed = (yDifference * speed) / length * -1;
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