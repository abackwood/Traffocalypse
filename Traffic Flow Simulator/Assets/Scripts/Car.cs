using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Car : MonoBehaviour
{
    public delegate void CarEventHandler(Car car);

    public event CarEventHandler Spawned, ReachedDestination;

    public Transform collision;

    public Lane currentLane;
    public SourceSink source, destination;
    public float distanceOnLane;
    public Car nextCar;

    public CarState state;
    public float speed;

    public CarAI ai;

    // Use this for initialization
    void Start()
    {
        ai = new CarAI(this);
        ai.route_index = -1;

        currentLane.Subscribe(this);

        if (Spawned != null)
        {
            Spawned(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsAtDestination() && ReachedDestination != null)
        {
            ReachedDestination(this);
        }

        ai.Update();
        Move();
    }

    bool IsAtDestination()
    {
        return currentLane.to == destination && currentLane.length - distanceOnLane <= 0;
    }

    void StartNewLane()
    {
    }

    void Move()
    {
        float trueSpeed = speed * Time.deltaTime;
        Vector3 movement = currentLane.direction * trueSpeed;

        Intersection intersection = currentLane.to as Intersection;

        if (intersection != null)
        {
            float distanceToTurn = Vector3.Distance(transform.position, ai.NextTurn.TurnPoint);
			ExplicitTurn nextTurn = ai.NextTurn;

            //If a turn point will be reached special actions need to be taken
            if (movement.magnitude > distanceToTurn)
            {
				Lane newLane = nextTurn.LaneOut;	//Find new lane
				float newDistanceOnLane = Vector3.Distance(nextTurn.TurnPoint, newLane.startPoint);

                //Move position and adjust car location internally
				transform.position = nextTurn.TurnPoint;	//Drive to next turn
				distanceOnLane = -newDistanceOnLane;		//You start somewhat behind the start of the lane
                SwitchToLane(newLane);

                ai.route_index++;

                //Recalculate speed and movement
                trueSpeed = movement.magnitude - distanceToTurn;	//Speed is now the rest after driving to the turn
                movement = currentLane.direction * trueSpeed;	//Recalculate movement
            }
        }

        distanceOnLane += trueSpeed;
        transform.Translate(movement);
    }
    void SwitchToLane(Lane lane)
    {
        currentLane.Unsubcribe(this);
        currentLane = lane;
        currentLane.Subscribe(this);
    }

    public void OnTriggerEnter2D(Collider2D col)
    {
        GameObject car = col.gameObject;
        Car carScript = col.GetComponent<Car>();
        if (carScript != null)
            if (carScript.distanceOnLane > distanceOnLane)
                Instantiate(collision, transform.position, Quaternion.identity);
        currentLane.Unsubcribe(this);
        currentLane.UnsubscribeFromQ(this);
        Destroy(gameObject);
    }
}

public enum CarState
{
    DRIVING,
    QUEUED,
    ON_INTERSECTION
}
