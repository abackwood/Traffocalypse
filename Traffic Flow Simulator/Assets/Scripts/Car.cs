﻿using UnityEngine;
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

    public float angerState;

    public CarState state;
	public Vector3 direction;
    public float speed;

    public CarAI ai;

    SpriteRenderer renderer;
    AudioSource audio;
    bool isHonking = false;
    float timeHonking = 0;

    // Use this for initialization
    void Start()
    {
        ai.route_index = -1;

        if (Spawned != null)
        {
            Spawned(this);
        }

        renderer = GetComponent<SpriteRenderer>();
        audio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsAtDestination() && ReachedDestination != null)
        {
            ReachedDestination(this);
        }

        angerState = AngerState;

        ai.Update();
        Move();

        //Changes to color from red to green depending on the anger state
        if (!isHonking)
            renderer.color = new Color(angerState, (1-angerState), 0);

        if (timeHonking > 1.0f)
            isHonking = false;

        timeHonking += Time.deltaTime;
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
        Vector3 movement = direction * trueSpeed;

        /*Intersection intersection = currentLane.to as Intersection;

        if (intersection != null)
        {
            float distanceToTurn = Vector3.Distance(transform.position, ai.nextTurn.TurnPoint);
			ExplicitTurn nextTurn = ai.nextTurn;

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
				ai.SelectNextTurn();

                //Recalculate speed and movement
                trueSpeed = movement.magnitude - distanceToTurn;	//Speed is now the rest after driving to the turn
                movement = direction * trueSpeed;	//Recalculate movement
            }
        }*/

        distanceOnLane += trueSpeed;
        transform.Translate(movement);
    }
    public void SwitchToLane(Lane lane)
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
            if (carScript.distanceOnLane < distanceOnLane)
            {
                Transform collisionObject = Instantiate(collision, transform.position, Quaternion.identity) as Transform;
                Collision collisionScript = collisionObject.GetComponent<Collision>();
                currentLane.ReplaceCar(this, collisionScript);
                collisionScript.distanceOnLane = distanceOnLane;
                collisionScript.currentLane = currentLane;

				collisionScript.InitCollision();
            }
        GameObject spawnerObject = GameObject.Find("Spawner");
        CarSpawner carSpawner = spawnerObject.GetComponent<CarSpawner>();
        carSpawner.carsCrashed++;
        currentLane.Unsubcribe(this);
        currentLane.UnsubscribeFromQ(this);
        Destroy(gameObject);
    }

    public void Honk()
    {
        audio.Play();
        renderer.color = new Color(255, 255, 0);
        timeHonking = 0;
        isHonking = true;
    }

    public float AngerState
    {
        get { return ai.anger_state; }
    }

	public override bool Equals (object o)
	{
		return this == o;
	}
}

public enum CarState
{
    DRIVING,
    QUEUED,
    HEADING_TO_TURN,
	AWAY_FROM_TURN
}
