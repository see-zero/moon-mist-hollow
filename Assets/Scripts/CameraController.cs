﻿using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float panSpeed = 24f;
    public float zoomSpeed = 10f;

    void Start()
    {
    }

    void Update()
    {
        float dx = panSpeed * Time.deltaTime * Input.GetAxis("Horizontal");
        float dy = panSpeed * Time.deltaTime * Input.GetAxis("Vertical");

        Camera.main.transform.Translate(dx, dy, 0);

        float dz = zoomSpeed * Time.deltaTime * Input.GetAxis("Zoom");
        
        Camera.main.orthographicSize += dz;
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 3f, 26f);
    }
}
