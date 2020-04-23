using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Security.Policy;

public class Truck : MonoBehaviour
{
    public Transform target;

    private GameObject load;

    private bool moving_to_target = false;
    private bool moving_to_source = false;

    private Moving _mov;
    private ResourceContainer _cont;

    // Start is called before the first frame update
    void Awake()
    {
        _mov = GetComponent<Moving>();
        _cont = GetComponent<ResourceContainer>();
        load = transform.Find("Mesh").Find("Load").gameObject;

        _cont.OnPickUp += Cont_OnPickUp;
        _cont.OnDropOff += Cont_OnDropOff;

        GetComponent<Unit>().OnActivate += Unit_OnActivate;
    }

    private void Unit_OnActivate(object sender, System.EventArgs e)
    {
        FindSource();
    }

    private void Cont_OnDropOff(object sender, System.EventArgs e)
    {
        load.SetActive(false);
        FindSource();
        moving_to_source = true;
        moving_to_target = false;
    }

    private void Cont_OnPickUp(object sender, System.EventArgs e)
    {
        load.SetActive(true);
        _mov.SetDestination(target.position);
        moving_to_source = false;
        moving_to_target = true;
    }

    public void FindSource()
    {
        Debug.Log("Looking for Mine!");
        GameObject[] resources = GameObject.FindGameObjectsWithTag("Mine");

        Debug.Log("Options:" + resources.Length);

        GameObject mine = null;
        float min_score = float.MaxValue;
        foreach (GameObject resource in resources)
        {
            if (resource.GetComponent<Unit>().team != GetComponent<Unit>().team)
                continue;

            float dist = (transform.position - resource.transform.position).magnitude;
            int metal = resource.GetComponent<ResourceContainer>().metal;
            if (metal == 0)
                metal = 1;
            float score = dist * (1 / metal);

            if (score <= min_score)
            {
                min_score = score;
                mine = resource;
            }
        }
        if (mine == null)
            return;

        ResourceContainer cont = mine.GetComponent<ResourceContainer>();
        cont._po_points_idx = (cont._po_points_idx + 1 ) / cont._pick_off_points.Length;
        Transform source = cont._pick_off_points[cont._po_points_idx];
        _mov.SetDestination(source.position, source.rotation);
    }
}
