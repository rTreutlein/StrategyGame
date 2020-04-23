using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using Panda;

public class ResourceContainer : MonoBehaviour
{
    public int metal;

    public int max_metal;

    public int metal_rate;

    public bool allow_dropoff;
    public bool allow_pickup;

    public event EventHandler OnPickUp;
    public event EventHandler OnDropOff;

    public Transform[] _pick_off_points;
    public int _po_points_idx = 0;

    private List<ResourceContainer> waiting = new List<ResourceContainer>();

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ResourceUpdate());
        Assert.IsFalse(allow_dropoff && allow_pickup);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator ResourceUpdate()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            metal += metal_rate;

            metal = Mathf.Clamp(metal, 0, max_metal);

            if (waiting.Count > 0)
                if ((allow_pickup && PickUp(waiting[0])) || (allow_dropoff && DropOff(waiting[0])))
                    waiting.RemoveAt(0);
        }
    }

    private bool PickUp(ResourceContainer cont)
    {
        int ammount = cont.max_metal - cont.metal;
        if (ammount > metal)
            return false;

        cont.metal += ammount;
        metal -= ammount;
        cont.OnPickUp(this, EventArgs.Empty);
        return true;
    }

    private bool DropOff(ResourceContainer cont)
    {
        int ammount = max_metal - metal;
        if (cont.metal == 0 || ammount < cont.metal)
            return false;

        metal += cont.metal;
        cont.metal = 0;
        cont.OnDropOff(this, EventArgs.Empty);
        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((allow_dropoff || allow_pickup) && other.tag == "ResourceCarrier")
        {
            ResourceContainer info = other.GetComponent<ResourceContainer>();
            if ((allow_pickup && !PickUp(info)) || (allow_dropoff && !DropOff(info)))
                    waiting.Add(info);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((allow_dropoff || allow_pickup) && other.tag == "ResourceCarrier")
        {
            ResourceContainer info = other.GetComponent<ResourceContainer>();
            waiting.Remove(info);
        }
    }

    [Task]
    public bool HasMetal(int ammount)
    {
        return metal >= ammount;
    }

}
