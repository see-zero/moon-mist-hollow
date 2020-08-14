using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float speed = 24f;
    public float zoomRate = 10f;

    void Start()
    {
    }

    void Update()
    {
        float dx = speed * Input.GetAxis("Horizontal");
        float dy = speed * Input.GetAxis("Vertical");

        dx *= Time.deltaTime;
        dy *= Time.deltaTime;

        float dz = zoomRate * Input.GetAxis("Zoom");
        
        dz *= Time.deltaTime;

        Camera.main.transform.Translate(dx, dy, 0);        

        Camera.main.orthographicSize += dz;
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 3f, 26f);
    }
}
