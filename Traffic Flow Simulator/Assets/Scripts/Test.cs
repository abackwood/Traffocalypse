using UnityEngine;
using System.Collections;
using System.IO;

public class Test : MonoBehaviour 
{
    public int testTime = 300;
    public int amountOfTests = 10;
    public int currentTests = 0;
    public PlayerAI playerAI;
    public int testMode = 1;

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
        Application.LoadLevel("DriveCity");
        Invoke("OutputTestResults", testTime);
        Debug.Log("Invoked in " + testTime + "  seconds");
    }

    void OutputTestResults()
    {
        GameObject spawnerObject = GameObject.Find("Spawner");
        carSpawner = spawnerObject.GetComponent<CarSpawner>();
        writer.WriteLine(carSpawner.carsSpawned + "," + carSpawner.carsCompleted + "," + carSpawner.carsCrashed);
        Debug.Log("write text");
        currentTests++;
        if (currentTests < amountOfTests)
            LoadScene();
        else
        {
            GameObject obj = GameObject.Find("PlayerAI");
            playerAI = obj.GetComponent<PlayerAI>();
            if (playerAI.testMode < 4)
            {
                testMode++;
                currentTests = 0;
                LoadScene();
            }
        }
    }
}
