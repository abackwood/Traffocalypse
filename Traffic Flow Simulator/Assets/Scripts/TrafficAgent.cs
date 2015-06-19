using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class TrafficAgent : MonoBehaviour {
	public Intersection intersection;
	public int status;
	private float greenTime = 4;
	public float timeSpan;
	public List<Lane> lanes;
	public Slider sliderPrefab;
	public List<Slider> sliders;
	// TODO a better way to position the slider set
	public Vector3 initSliderPos = new Vector3(10, 10, 0);
	

	// Use this for initialization
	void Start () {
		int y = 0;
		foreach (Road r in intersection.roads) {
			foreach(Lane l in r.InLanes(intersection)){
				lanes.Add(l);
				Slider clone = (Slider)Instantiate(sliderPrefab, initSliderPos+ new Vector3(0,y,0),Quaternion.identity);
				GameObject parent = GameObject.Find("Canvas");
				clone.transform.parent = parent.transform;
				sliders.Add(clone);
				y += 5;
			}
		}
		status = 0;
		timeSpan = greenTime;
	}
	
	// Update is called once per frame
	void Update () {
		timeSpan -= Time.deltaTime;
		if (timeSpan < 0) {

			timeSpan = greenTime;
			lanes[status].intersectionOpen = false;

			if (status == lanes.Count - 1) 
				status = 0;
			else
				status += 1;

			lanes[status].intersectionOpen = true;
		}

		
	}
}
