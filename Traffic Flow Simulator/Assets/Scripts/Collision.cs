using UnityEngine;
using System.Collections;

public class Collision : Car
{
    public TowTruck towTruck;

	void Start () 
    {
        GameObject obj = GameObject.Find("Canvas/TowAwayText");
        towTruck = obj.GetComponent<TowTruck>();
        GameObject playerAI = GameObject.Find("PlayerAI");
        if (playerAI != null)
        {
            PlayerAI playerAIScript = playerAI.GetComponent<PlayerAI>();
            playerAIScript.AddCollision(this);
        }
	}

	public void InitCollision () {
		currentLane.blocked = true;
		currentLane.GetComponent<LineRenderer>().SetColors(Color.red, Color.red);
		
		GameObject spawner_obj = GameObject.Find ("Spawner");
		CarSpawner spawner = spawner_obj.GetComponent<CarSpawner>();
		foreach(Car c in spawner.cars) {
			c.ai.route_index = -1;
		}
	}
	
	void Update () 
    {
		
	}

    public void OnTriggerEnter2D(Collider2D col)
    {
    }

    void OnMouseDown()
    {
        towTruck.StartTowing(this);
    }

    public void Remove()
    {
		currentLane.blocked = false;
		currentLane.GetComponent<LineRenderer>().SetColors(Color.black, Color.black);
        currentLane.Unsubcribe(this);

		GameObject spawner_obj = GameObject.Find ("Spawner");
		CarSpawner spawner = spawner_obj.GetComponent<CarSpawner>();
		foreach(Car c in spawner.cars) {
			c.ai.route_index = -1;
		}

        Destroy(gameObject);
    }
}
