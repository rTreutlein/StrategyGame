using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid
{
    private int _width;
    private int _height;
    private int[,] _grid;

    public Grid(int width, int height, Vector3 center)
    {
        _width = width;
        _height = height;
        _grid = new int[_width, _height];


    }

}
