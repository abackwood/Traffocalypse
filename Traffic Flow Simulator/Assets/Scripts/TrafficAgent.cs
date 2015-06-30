using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class TrafficAgent : MonoBehaviour {
	public Intersection intersection;
	public int status;
	private float TotalGreenTime = 10;

	public float timeSpan; // TODO is assigned depending on priority
	public List<Lane> lanes; //
	public Slider sliderPrefab;
	public List<Slider> sliders;
	// TODO a better way to position the slider set
	public Vector3 initSliderPos = new Vector3(10, 10, 0);
	public IntersectionModel intmodel;
	public Priority priority;
	private bool coolDown;
	private float coolDownTime = 1;
	

	// Use this for initialization
	void Start () {
		intmodel = new IntersectionModel (intersection);
		priority = new Priority (intmodel);
		coolDown = false;

		//TODO redo completely with the new thing in mind
		/*int y = 0;
		foreach (Road r in intersection.roads) {
			foreach(Lane l in r.InLanes(intersection)){
				lanes.Add(l);
				Slider clone = (Slider)Instantiate(sliderPrefab, initSliderPos+ new Vector3(0,y,0),Quaternion.identity);
				GameObject parent = GameObject.Find("Canvas");
				clone.transform.parent = parent.transform;
				sliders.Add(clone);
				y += 5;
			}
		}*/
		status = 0;
		timeSpan = TotalGreenTime * priority.priorityList[0];
	}

	void Update () {
		timeSpan -= Time.deltaTime;
		if (timeSpan < 0) {
			if(coolDown){

				priority.Update();
				
				status = (status + 1)%intmodel.phasesNumber;
				timeSpan = TotalGreenTime * priority.priorityList[status];
				
				intersection.SetOpen(intmodel.phases[status]);
			}
			else
			{
				timeSpan = coolDownTime;
				intersection.SetOpen(new List<PossibleTurn>());
				
			}
			coolDown = !coolDown;

		}

		
	}

	// TODO priority structure
	// first Longest-Q fixed time ---> stupid
	// second Longest-Q proportional time ---> done
	// third JTA :')

	public struct Priority{
		public List<float> priorityList;
		private IntersectionModel intmodel;

		public Priority(IntersectionModel intmodel){
			this.intmodel = intmodel;

			priorityList = new List<float>();

			for(int i = 0; i < intmodel.phasesNumber; i++){
				priorityList.Add(1/intmodel.phasesNumber);
			}
		}

		public void Update(){
			List<int> currentQs = intmodel.getQsLength ();
			//Debug.Log ("Printing current priority values: ");
			// currentWs.Sum() should be there, but isn't
			int totalLength = 0;
			foreach (int i in currentQs) {
				totalLength += i;
			}
			//Debug.Log("Total length: " + totalLength.ToString());
			if (totalLength > 0) {
				for(int i = 0; i < priorityList.Count; i++){
					priorityList[i] = (float)currentQs[i]/totalLength;
					//Debug.Log("CurrentQ " + i.ToString() + ": " + currentQs[i].ToString());
					//Debug.Log (((float)currentQs[i]/totalLength).ToString ());
				}
			} else {
				for(int i = 0; i < priorityList.Count; i++){
					priorityList[i] = 1f/intmodel.phasesNumber;
				}
			}

		}
	}

	public struct IntersectionModel{
		public List<List<PossibleTurn>> phases;
		public int phasesNumber;

		public IntersectionModel(Intersection intersection){

			this.phases = new List<List<PossibleTurn>>();
			
			int inroads = 0;
			int outroads = 0;
			
			// length is 4 anyways
			for (int i = 0; i < intersection.roads.Length; i++){
				
				if(intersection.roads[i].InLanes(intersection).Length > 0)
					inroads++;
				
				if(intersection.roads[i].OutLanes(intersection).Length > 0)
					outroads++;
			}
			// TODO generalize to a range of numbers of roads
			if (intersection.roads.Length == 4){
				if(outroads == 4){
					if(inroads == 4){

						for (int i = 0; i < 2; i++){
							List<PossibleTurn> ls = new List<PossibleTurn>();

							// Turning right
							ls.Add(new PossibleTurn(intersection.roads[i].InLanes(intersection)[0], intersection.roads[i+1].OutLanes(intersection)));
							ls.Add(new PossibleTurn(intersection.roads[i+2].InLanes(intersection)[0], intersection.roads[(i+1)%4].OutLanes(intersection)));

							// Going straight ahead
							for (int j = 0; j < 2; j++){
								if (intersection.roads[i + (j*2)].InLanes(intersection).Length > 2){
									for (int k = 1; k < intersection.roads[i + (j*2)].InLanes(intersection).Length-1; k++){
										ls.Add(new PossibleTurn(intersection.roads[i + (j*2)].InLanes(intersection)[k], intersection.roads[(i+(j*2)+2)%4].OutLanes(intersection)));
									}
								}
								else{
									ls.Add(new PossibleTurn(intersection.roads[i + (j*2)].InLanes(intersection)[0], intersection.roads[(i+(j*2)+2)%4].OutLanes(intersection)));
									}
							}

							this.phases.Add(ls);

						}
					


						// Turning left
						for (int i = 0; i<2; i++){
							List<PossibleTurn> ls = new List<PossibleTurn>();

							ls.Add(new PossibleTurn(intersection.roads[i].InLanes(intersection)[intersection.roads[i].InLanes(intersection).Length-1], intersection.roads[(i+3)%4].OutLanes(intersection)));
							ls.Add(new PossibleTurn(intersection.roads[i+2].InLanes(intersection)[intersection.roads[i+2].InLanes(intersection).Length -1], intersection.roads[(i+5)%4].OutLanes(intersection)));

							this.phases.Add(ls);
						}

					}
				}
			}
			// TODO implement T intersection and the other types as well
			else if (intersection.roads.Length == 3)
				;
			else{
				foreach(Road r in intersection.roads){
					List<PossibleTurn> ls = new List<PossibleTurn>();
					foreach(Lane l in r.InLanes(intersection)){
						foreach(Road s in intersection.roads){
							if (r != s){
								ls.Add (new PossibleTurn (l, s.OutLanes(intersection)));
								foreach(Lane m in s.OutLanes(intersection)){
								}
							}
						}
					}
					if(ls.Count>0)
						this.phases.Add(ls);
				}
			}

			this.phasesNumber = this.phases.Count;
		}

		public List<int> getQsLength(){
			List<int> QsLength = new List<int> ();

			foreach (List<PossibleTurn> p in phases) {
				int thisLength = 0;
				foreach(PossibleTurn q in p){
					thisLength += q.LaneIn.GetCarsAtIntersection();
				}
				QsLength.Add(thisLength);
			}
			return QsLength;
		}
	

	}

	// TODO intersection type structure aka INTERSECTION MODEL
	// There are 5 major intersection types, for a 4-way intersection:
	// - all the roads are 2-way
	// - 1 road is 1-way
	// - 2 opposite roads are 1-way
	// - 2 adjacent roads are 1-way
	// - 3 roads are 1 way
	// Set a best lane distribution for each possible intersection structure
	// Set a best phase scheme for each possible intersection structure
	// 
}
