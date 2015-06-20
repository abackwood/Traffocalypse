using UnityEngine;
using System.Collections;

public class Collision : MonoBehaviour 
{
    public float towAwayTime = 5;
    public GameObject tower;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnMouseDown()
    {
        Invoke("TowAway", towAwayTime);
        GameObject tower = GameObject.Find("TowAwayText");
        GUIText text = tower.GetComponent<GUIText>();
        text.text = "Car currently being towed: yes";
    }

    void TowAway()
    {
        Destroy(gameObject);
    }
}
