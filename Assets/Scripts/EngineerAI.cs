using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;
using Panda.Examples.Shooter;
using System.Runtime.InteropServices;

public class EngineerAI : MonoBehaviour
{
    public string _task;

    private AIManager _ai_mgr;
    private Builder _builder;

    private Vector3 _dest;

    // Start is called before the first frame update
    void Start()
    {
        _ai_mgr = GameObject.FindObjectOfType<AIManager>();
        _builder = GetComponent<Builder>();

        if (_ai_mgr == null)
        {
            Debug.Log("Is NULL?");
            Debug.Break();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    [Task]
    public bool HasTask()
    {
        return _task != "";
    }

    [Task]
    public bool TaskIs(string cmp)
    {
        return _task == cmp;
    }

    [Task]
    public bool GetTask()
    {
        _task = _ai_mgr.GetTask();
        if (_task == "Expand")
        {
            _dest = _ai_mgr.GetFreeExpansion();
        }
        return true;
    }

    [Task]
    public bool GoToDest()
    {
        GetComponent<Moving>().SetDestination(_dest);
        return true;
    }

    [Task]
    public bool AtDest()
    {
        return (transform.position - _dest).magnitude < 1;
    }

    [Task]
    public bool ReturnBorrowed()
    {
        foreach (GameObject truck in _builder._trucks)
        {
            truck.GetComponent<Truck>().target = _builder._parent.transform;
        }

        _builder._parent._trucks.AddRange(_builder._trucks);
        _builder._trucks = new List<GameObject>();
        return true;
    }

    [Task]
    public bool BorrowTruck()
    {
        _builder._trucks.Add(_builder._parent._trucks[0]);
        _builder._parent._trucks.RemoveAt(0);
        _builder._trucks[0].GetComponent<Truck>().target = transform;

        return true;
    }

    [Task]
    public bool NeedUnits()
    {
        if (GetComponent<AttackManager>()._units.Count >= 2)
            return false;

        if (_task == "Expand")
        {
            return true;
        }
        return false;
    }

    [Task]
    public bool GetUnits()
    {
        AttackManager atk_mgr = _builder._parent.GetComponent<AttackManager>();
        GetComponent<AttackManager>()._units.Add(atk_mgr._units[0]);
        atk_mgr._units.RemoveAt(0);
        return true;
    }

}
