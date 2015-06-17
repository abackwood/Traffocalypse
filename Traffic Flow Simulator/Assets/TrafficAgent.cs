using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrafficAgent : MonoBehaviour {
	public Intersection intersection;
	public int status;
	private float greenTime = 4;
	public float timeSpan;
	public List<Lane> lanes;

	

	// Use this for initialization
	void Start () {
		/*foreach (Road r in intersection.roads) {
			foreach(Lane l in r.InLanes(intersection)){
				lanes.Add(l);
			}
		}*/
		status = 0;
		timeSpan = greenTime;
	}
	
	// Update is called once per frame
	void Update () {
		timeSpan -= Time.deltaTime;
		if (timeSpan < 0) {

			timeSpan = greenTime;
			//lanes[status].intersectionOpen = false;

			if (status == lanes.Count - 1) 
				status = 0;
			else
				status += 1;

			//lanes[status].intersectionOpen = true;
		}

		
	}
}
