using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTesting : MonoBehaviour
{
    private Grid _grid;

    void Start()
    {
        _grid = new Grid(10, 10, Vector3.zero);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
