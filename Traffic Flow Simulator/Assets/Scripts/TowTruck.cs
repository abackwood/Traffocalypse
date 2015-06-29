using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TowTruck : MonoBehaviour 
{
    public float towAwayTime = 5;
    public Text textObject;
    public Collision currentCollision;
    public bool busy = false;

	// Use this for initialization
	void Start () 
    {
        textObject = gameObject.GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    public void StartTowing(Collision collision)
    {
        if (collision == currentCollision)
            return;
        textObject.text = "Truck in use: yes";
        currentCollision = collision;
        CancelInvoke("EndTowing");
        Invoke("EndTowing", towAwayTime);
        busy = true;
    }

    public void EndTowing()
    {
        textObject.text = "Truck in use: no";
        currentCollision.Remove();
        currentCollision = null;
        busy = false;
    }
}
