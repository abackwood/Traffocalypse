using UnityEngine;
using System.Collections;
using System.IO;

public class Test : MonoBehaviour 
{
    public int testTime = 30;

    private StreamWriter writer;
    private CarSpawner carSpawner;

	void Start () 
    {
        Object.DontDestroyOnLoad(gameObject);
        writer = new StreamWriter("testResults.txt");
        writer.AutoFlush = true;
        LoadScene();
	}
	
	void Update () 
    {
	
	}

    void LoadScene()
    {
        Application.LoadLevel("MainScene");
        Invoke("OutputTestResults", testTime);
        Debug.Log("Invoked in " + testTime + "  seconds");
    }

    void OutputTestResults()
    {
        GameObject spawnerObject = GameObject.Find("Spawner");
        carSpawner = spawnerObject.GetComponent<CarSpawner>();
        writer.WriteLine(carSpawner.carsSpawned + "," + carSpawner.carsCompleted + "," + carSpawner.carsCrashed);
        Debug.Log("write text");
        LoadScene();
    }
}
