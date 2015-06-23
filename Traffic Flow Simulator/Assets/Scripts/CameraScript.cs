using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour 
{
    public float min = -10;
    public float max = 10;
    public float scrollSpeed = 10f;

    public float moveDistance = 10;
    public float moveSpeed = 10f;

	// Use this for initialization
	void Start () 
    {
        min = Camera.main.orthographicSize + min;
        max = Camera.main.orthographicSize + max;
	}
	
	// Update is called once per frame
	void Update () 
    {
        Camera.main.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
        if (Camera.main.orthographicSize < min)
            Camera.main.orthographicSize = min;
        if (Camera.main.orthographicSize > max)
            Camera.main.orthographicSize = max;

        if (Input.mousePosition.x < moveDistance)
            Camera.main.transform.Translate(-moveSpeed, 0, 0);
        if (Input.mousePosition.x > Screen.width - moveDistance)
            Camera.main.transform.Translate(moveSpeed, 0, 0);
        if (Input.mousePosition.y < moveDistance)
            Camera.main.transform.Translate(0, -moveSpeed, 0);
        if (Input.mousePosition.y > Screen.height - moveDistance)
            Camera.main.transform.Translate(0, moveSpeed, 0);
	}
}
