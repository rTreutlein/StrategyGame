using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Threading;
using TMPro;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;
using UnityEngine.Events;

using Panda;
public class Builder : MonoBehaviour
{
    [Header("Attributes")]
    public int buildRange = 10;
    public float build_power = 5;
    public int reclaim_speed;

    [Header("Prefas")]
    public GameObject prefab_Truck;
    public GameObject prefab_Engineer;
    public GameObject prefab_Mine;
    public GameObject prefab_Tank;
    public GameObject prefab_Arty;
    public GameObject prefab_Tower;

    //Building Related
    private string _action = "";
    private bool is_building;
    private Coroutine co_building;
    private GameObject being_built;

    //Reclaim Related
    private bool is_reclaiming;
    private GameObject rec_obj;
    private Coroutine co_reclaim;

    //Components
    private LineRenderer _line;
    private ResourceContainer _cont;
    private Unit _unit;
    private AttackManager _atk_mngr;

    //For Ai
    private int idx_buildord = 0;

    //Object Dictonries
    private Dictionary<String, GameObject> prefabs = new Dictionary<string, GameObject>();

    //Supporting Units
    public Builder _parent;
    public List<GameObject> _trucks = new List<GameObject>();
    public List<GameObject> _mines = new List<GameObject>();

    //Keep Track of spending
    private int _spent_eco = 0;
    private int _spent_atk = 0;

    private WaitForSeconds waitOneSecond;

    public bool Is_building { get => is_building; private set => is_building = value; }

    // Start is called before the first frame update
    void Start()
    {
        _line = GetComponent<LineRenderer>();
        _unit = GetComponent<Unit>();
        _cont = GetComponent<ResourceContainer>();
        _atk_mngr = GetComponent<AttackManager>();
        waitOneSecond = new WaitForSeconds(1);

        _line.SetPosition(0, transform.Find("Mesh").Find("Sphere").position);
        _line.SetPosition(1, transform.Find("Mesh").Find("Sphere").position);
        _line.enabled = false;

        prefabs["Truck"] = prefab_Truck;
        prefabs["Engineer"] = prefab_Engineer;
        prefabs["Mine"] = prefab_Mine;
        prefabs["Tank"] = prefab_Tank;
        prefabs["Arty"] = prefab_Arty;
        prefabs["Tower"] = prefab_Tower;
    }

    // Update is called once per frame
    void Update()
    {
        if (_line.enabled)
            _line.SetPosition(0, transform.Find("Mesh").Find("Sphere").position);
    }

    public void Reclaim(GameObject gameObject)
    {
        is_reclaiming = true;
        rec_obj = gameObject;
        co_reclaim = StartCoroutine(Co_Reclaim());
    }

    IEnumerator Co_Reclaim()
    {
        _line.enabled = true;
        _line.SetPosition(1, rec_obj.transform.position);

        Wreck wreck = rec_obj.GetComponent<Wreck>();
        int steps = wreck.metal / reclaim_speed;

        for (int i = 0; i < steps; i++)
        {
            Vector3[] verts = rec_obj.GetComponent<MeshFilter>().mesh.vertices;
            Vector3 pos = verts[UnityEngine.Random.Range(0, verts.Length)];
            pos = rec_obj.transform.TransformPoint(pos);
            _line.SetPosition(1, pos);

            _cont.metal += reclaim_speed;
            yield return waitOneSecond;
        }

        StopCoroutine(co_reclaim);
        is_reclaiming = false;
        _line.enabled = false;
        Destroy(rec_obj);
        yield break;
    }

    private GameObject FindClosest(string tag, Func<GameObject, bool> filter, Func<GameObject, GameObject, bool> pred)
    {
        GameObject[] resources = GameObject.FindGameObjectsWithTag(tag);
        GameObject found = null;
        float min_dist = float.MaxValue;
        foreach (GameObject resource in resources)
        {
            if (!filter(resource))
                continue;
            float dist = (transform.position - resource.transform.position).magnitude;
            if (dist < min_dist && (found == null || pred(found, resource)))
            {
                min_dist = dist;
                found = resource;
            }
        }
        return found;
    }

    private Guid task_id;

    [Task]
    public void AiBuild(String action)
    {
        _action = action;
        if (Task.current.isStarting && !Is_building)
        {
            Debug.Log("First");
            task_id = Guid.NewGuid();
            Task.current.item = task_id;
        }
        else if (Is_building)
        {
            return;
        }
        else if ((Guid)Task.current.item == task_id)
        {
            Task.current.Succeed();
            return;
        }

        Vector3 loc = transform.position + (Vector3.forward * 3); ;
        if (_action == "Mine")
        {
            GameObject resoruce = FindClosest("Resource"
                                             , (g) =>
                                             { return g.GetComponent<Metal>().is_free; }
                                             , (_, g) => { return true; });
            if (resoruce != null)
            {
                loc = resoruce.transform.position;
                loc.y += 0.4f;
                resoruce.GetComponent<Metal>().is_free = false;
            }
        }

        Debug.Log("Building: " + _action);
        GameObject inst = Instantiate(prefabs[_action], loc, Quaternion.identity);
        Build(inst);
    }

    public GameObject PreBuild(String action, Vector3 loc)
    {
        _action = action;
        return Instantiate(prefabs[_action], loc, Quaternion.identity);
    }

    public void Build(GameObject inst)
    {
        being_built = inst;
        Is_building = true;
        co_building = StartCoroutine(Co_Build());
        Unit inst_unit = inst.GetComponent<Unit>();
        inst_unit.team = _unit.team;

        if (_action == "Truck")
        {
            Debug.Log("Added Transorm.");
            inst.GetComponent<Truck>().target = transform;
        }

        if (_action == "Engineer")
        {
            inst.GetComponent<Builder>()._parent = this;
        }
    }

    IEnumerator Co_Build()
    {
        _line.enabled = true;
        _line.SetPosition(1, being_built.transform.position);

        Unit unit = being_built.GetComponent<Unit>();
        int steps = unit.cost / unit.build_rate;

        for (int i = 1; i <= steps; i++)
        {
            while (_cont.metal < unit.build_rate)
                yield return waitOneSecond;
            Transform mesh = being_built.transform.Find("Mesh");
            Vector3[] verts = mesh.GetComponent<MeshFilter>().mesh.vertices;
            Vector3 pos = verts[UnityEngine.Random.Range(0, verts.Length)];
            pos = being_built.transform.TransformPoint(pos);
            _line.SetPosition(1, pos);

            MaterialPropertyBlock properties = new MaterialPropertyBlock();
            properties.SetFloat("Transparency", (float)i / steps);

            Debug.Log("i,steps: " + i + "," + steps);
            Debug.Log("Transparency: " + ((float)i / steps));

            Renderer renderer = mesh.GetComponent<Renderer>();
            int ammount = renderer.materials.Length;
            for (int j = 0; j < ammount; j++)
            {
                renderer.SetPropertyBlock(properties,j);
            }

            _cont.metal -= unit.build_rate;
            unit.AddHealth(unit.max_health * (unit.build_rate / unit.cost));

            yield return waitOneSecond;
        }

        BuildDone(unit);
        StopCoroutine(co_building);
        yield break;
    }

    private void BuildDone(Unit unit)
    {
        Debug.Log("Built: " + _action);

        if (being_built.GetComponent<Tank>() != null)
        {
            _atk_mngr.AddUnit(being_built.GetComponent<Moving>());
            _spent_atk += unit.cost;
        }
        else
        {
            _spent_eco += unit.cost;
        }

        if (_action == "Truck")
        {
            _trucks.Add(being_built);
        }
        if (_action == "Mine")
        {
            _mines.Add(being_built);
        }

        unit.Activate();
        Is_building = false;
        _line.enabled = false;
    }

    [Task]
    public bool Has(String what, int ammount)
    {
        if (what == "Truck")
            return _trucks.Count >= ammount;
        if (what == "Mine")
            return _mines.Count >= ammount;
        return false;
    }

    [Task]
    public bool MoreTanks()
    {
        return _spent_eco > (_spent_atk * 2);
    }

}
