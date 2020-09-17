using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class RaymarchHelper : MonoBehaviour
{
    [SerializeField] private Material material;
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
    }
    
    void Update()
    {
        material.SetMatrix("_CamRotationMatrix", Matrix4x4.Rotate(mainCamera.transform.rotation));
    }
}
