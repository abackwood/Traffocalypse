using UnityEngine;
using System.Collections;

public class Collision : Car
{
    public TowTruck towTruck;

	void Start () 
    {
        GameObject obj = GameObject.Find("Canvas/TowAwayText");
        towTruck = obj.GetComponent<TowTruck>();
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
        Destroy(gameObject);
    }
}
