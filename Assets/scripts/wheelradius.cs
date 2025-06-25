using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wradius : MonoBehaviour
{
    public MeshRenderer wheelMeshRenderer;
    void Start()
    {
        if (wheelMeshRenderer != null)
        {
            // Y-axis is vertical � adjust if needed
            float radius = wheelMeshRenderer.bounds.extents.y;
            Debug.Log("Wheel radius: " + radius + " units");
        }
        else
        {
            Debug.LogError("Wheel Mesh Renderer is not assigned.");
        }
    }
    public void getaxis()
    {

    }
}
