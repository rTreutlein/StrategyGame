using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuilderControler : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite sprt_Truck;
    public Sprite sprt_Engineer;
    public Sprite sprt_Mine;
    public Sprite sprt_Tank;
    public Sprite sprt_Arty;
    public Sprite sprt_Tower;
    public Sprite sprt_default;

    //Components
    private Projector projector;
    private Builder _builder;
    private Unit _unit;

    private GameObject _inst;

    private Dictionary<String, Sprite> sprits = new Dictionary<string, Sprite>();
    private Dictionary<String, Button> btns = new Dictionary<string, Button>();

    private String action;

    // Start is called before the first frame update
    void Start()
    {
        _builder = GetComponent<Builder>();
        _unit = GetComponent<Unit>();

        _unit.OnSelected += Unit_OnSelected;
        _unit.OnDeselected += Unit_OnDeselected;

        sprits["Truck"] = sprt_Truck;
        sprits["Engineer"] = sprt_Engineer;
        sprits["Mine"] = sprt_Mine;
        sprits["Tank"] = sprt_Tank;
        sprits["Arty"] = sprt_Arty;
        sprits["Tower"] = sprt_Tower;

        Transform sub = GameObject.FindObjectOfType<InputManager>().ui.transform.Find("btns");
        btns["Truck"] = sub.Find("btn2x1").GetComponent<Button>();
        btns["Engineer"] = sub.Find("btn2x2").GetComponent<Button>();
        btns["Mine"] = sub.Find("btn2x3").GetComponent<Button>();
        btns["Tank"] = sub.Find("btn3x1").GetComponent<Button>();
        btns["Arty"] = sub.Find("btn3x2").GetComponent<Button>();
        btns["Tower"] = sub.Find("btn3x3").GetComponent<Button>();
        projector = transform.Find("Projector").GetComponent<Projector>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_unit.Is_clicked)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo))
            {
                _inst.transform.position = hitInfo.point;

                if (Input.GetMouseButtonDown(0))
                {
                    if (DoAction(hitInfo))
                    {
                        _unit.Is_clicked = false;
                        Debug.Log("Set clicked ot false;");
                    }
                }
            }
        }

        if (!_unit.Is_clicked && Input.GetMouseButtonDown(1))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo))
            {
                if (hitInfo.collider.tag == "Scrap")
                {
                    Debug.Log("Reclaim!");
                    _builder.Reclaim(hitInfo.collider.gameObject);
                }
            }
        }

    }

    private bool DoAction(RaycastHit hitInfo)
    {
        Debug.Log("Trying to Build: " + action);

        if (Vector3.Distance(hitInfo.point, transform.position) > _builder.buildRange)
            return false;

        if (hitInfo.collider.tag == "Ground")
        {
            _inst.layer -= LayerMask.NameToLayer("Ignore Raycast");
            _builder.Build(_inst);
            return true;
        }

        if (action == "Mine" && hitInfo.transform.tag == "Resource" && hitInfo.transform.GetComponent<Metal>().is_free)
        {
            hitInfo.transform.GetComponent<Metal>().is_free = false;
            _inst.layer -= LayerMask.NameToLayer("Ignore Raycast");
            _builder.Build(_inst);
            return true;
        }

        return false;
    }

    private void Unit_OnDeselected(object sender, EventArgs e)
    {
        foreach (Button btn in btns.Values)
        {
            btn.GetComponent<Image>().sprite = sprt_default;
            btn.onClick.RemoveAllListeners();
        }
    }

    private void Unit_OnSelected(object sender, EventArgs e)
    {
        foreach (var item in btns)
        {
            item.Value.GetComponent<Image>().sprite = sprits[item.Key];
            item.Value.onClick.AddListener(() => BtnClick(item.Key));
        }
    }
    private void BtnClick(string value)
    {
        if (_builder.Is_building)
            return;
        _unit.Is_clicked = true;
        action = value;

        _inst = _builder.PreBuild(action, Vector3.zero);
        _inst.layer += LayerMask.NameToLayer("Ignore Raycast");
    }
}
