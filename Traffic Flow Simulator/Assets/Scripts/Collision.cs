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
        currentLane.Unsubcribe(this);
        Destroy(gameObject);
    }
}
