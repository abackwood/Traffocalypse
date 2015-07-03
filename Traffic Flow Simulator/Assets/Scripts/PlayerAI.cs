using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerAI : MonoBehaviour 
{
    public int testMode = 0;

    public GameObject towTruck;

    private List<Collision> collisionList;
    private TowTruck towTruckScript;

	public void Start () 
    {
        collisionList = new List<Collision>();
        towTruckScript = towTruck.GetComponent<TowTruck>() as TowTruck;
        
	}

    public void Awake()
    {
        Test test = GameObject.FindObjectOfType<Test>();
        //GameObject obj = GameObject.Find("TestObject");
        if (test != null)
        {
            //Test test = obj.GetComponent<Test>();
            testMode = test.testMode;
            Debug.Log(testMode);
        }
    }

	public void Update () 
    {
        if (!towTruckScript.busy)
            if(collisionList.Count > 0)
            {
                Collision collision = new Collision();
                if(testMode == 0)
                    collision = FindFirst();
                if (testMode == 1)
                    collision = FindLast();
                if (testMode == 2)
                    collision = FindRandom();
                if (testMode == 3)
                    collision = FindFastest();
                if (testMode == 4)
                    collision = FindSlowest();
                towTruckScript.StartTowing(collision);
            }
	}

    public void AddCollision(Collision collision)
    {
        collisionList.Add(collision);
    }

    private Collision FindFirst()
    {
        Collision collision = collisionList[0];
        collisionList.RemoveAt(0);
        return collision;
    }

    private Collision FindLast()
    {
        int number = collisionList.Count - 1;
        Collision collision = collisionList[number];
        collisionList.RemoveAt(number);
        return collision;
    }

    private Collision FindRandom()
    {
        int max = collisionList.Count - 1;
        float randomNumber = Random.value;
        int number = Mathf.CeilToInt(randomNumber * max);
        Collision collision = collisionList[number];
        collisionList.RemoveAt(number);
        return collision;
    }

    private Collision FindFastest()
    {
        int number = 0;
        for (int t = 1; t < collisionList.Count; t++)
            if (collisionList[t].currentLane.speedLimit > collisionList[number].currentLane.speedLimit)
                number = t;
        Collision collision = collisionList[number];
        collisionList.RemoveAt(number);
        return collision;
    }

    private Collision FindSlowest()
    {
        int number = 0;
        for (int t = 1; t < collisionList.Count; t++)
            if (collisionList[t].currentLane.speedLimit < collisionList[number].currentLane.speedLimit)
                number = t;
        Collision collision = collisionList[number];
        collisionList.RemoveAt(number);
        return collision;
    }
}
