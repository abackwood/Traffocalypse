using UnityEngine;
using System.Collections;

public class Car : MonoBehaviour 
{
    public Road road;
	public Lane currentLane;
	public SourceSink source, destination;
    public float distanceOnLane;
    public Car nextCar;

	// Use this for initialization
	void Start () 
    {
        currentLane = road.LanesForward[0];
        distanceOnLane = 0;
        currentLane.Subscribe(this);
	}
	
	// Update is called once per frame
	void Update () 
    {
        Move();
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
}