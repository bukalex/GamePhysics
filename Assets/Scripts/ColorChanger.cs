using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChanger : MonoBehaviour
{
    [SerializeField]
    private Color color = Color.white;

    void Awake()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer)
        {
            renderer.material.color = color;
        }
    }
}
