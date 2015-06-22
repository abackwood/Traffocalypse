using UnityEngine;
using System.Collections;

public class Collision : MonoBehaviour 
{
    public TowTruck towTruck;

	// Use this for initialization
	void Start () 
    {
        GameObject obj = GameObject.Find("Canvas/TowAwayText");
        towTruck = obj.GetComponent<TowTruck>();
	}
	
	// Update is called once per frame
	void Update () 
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
