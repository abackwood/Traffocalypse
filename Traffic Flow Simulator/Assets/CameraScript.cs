using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour 
{
    public float min = -10;
    public float max = 10;
    public float speed = 10f;

	// Use this for initialization
	void Start () 
    {
        min = Camera.main.orthographicSize + min;
        max = Camera.main.orthographicSize + max;
	}
	
	// Update is called once per frame
	void Update () 
    {
        Camera.main.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * speed;
        if (Camera.main.orthographicSize < min)
            Camera.main.orthographicSize = min;
        if (Camera.main.orthographicSize > max)
            Camera.main.orthographicSize = max;
	}
}
