using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TestNavscript : MonoBehaviour
{
    private NavMeshAgent agent;

    public Transform target;

    // Start is called before the first frame update
    void Start()
    {
        
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        agent.SetDestination(target.position);
    }
}
