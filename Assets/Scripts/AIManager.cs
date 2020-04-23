using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;
using Panda.Examples.Shooter;

public class AIManager : MonoBehaviour
{
    public GameObject expansion;

    private AIManager() { }

    public string GetTask()
    {
        return "Expand";
    }

    public Vector3 GetFreeExpansion()
    {
        return expansion.transform.position;
    }
}
