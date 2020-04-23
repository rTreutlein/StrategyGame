using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AtkMgrCtrl : MonoBehaviour
{
    private Unit _unit;
    private AttackManager _atk_mgr;
    private Button _btn;

    private Vector3 _pos_mouse;

    // Start is called before the first frame update
    void Start()
    {
        _atk_mgr = GetComponent<AttackManager>();
        _unit = GetComponent<Unit>();

        _unit.OnSelected += Unit_Selected;
        _unit.OnDeselected += Unit_Deselected;


        Transform btns = GameObject.FindObjectOfType<InputManager>().ui.transform.Find("btns");
        _btn = btns.Find("btn1x2").GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        LeftClick();  
        RightClick();
    }

    private void LeftClick()
    {

        if (_unit.Is_clicked && Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo))
            {
                _pos_mouse = hitInfo.point;
                GetComponent<LineRenderer>().SetPosition(0, _pos_mouse);
            }
        }
        if (_unit.Is_clicked && Input.GetMouseButton(0))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo))
            {
                GetComponent<LineRenderer>().SetPosition(1, _pos_mouse);
            }
        }
        if (!EventSystem.current.IsPointerOverGameObject() && _unit.Is_clicked && Input.GetMouseButtonUp(0))
        {
            MoveToMouse();
            _unit.Is_clicked = false;
        }
    }

    private void RightClick()
    {
        if (_unit.Is_selected && Input.GetMouseButtonDown(1))
        {
            if (_unit.Is_clicked)
            {
                _unit.Is_clicked = false;
                return;
            }
        }
    }

    private void MoveToMouse()
    {
        Debug.Log("Moving");

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo))
        {
            if (hitInfo.collider.tag == "Ground")
            {
                _atk_mgr.GetFormationPoints(_pos_mouse, hitInfo.point - _pos_mouse);
            }
        }
    }

    private void Unit_Deselected(object sender, System.EventArgs e)
    {
        //btn.GetComponent<Image>().sprite = sprt_default;
        _btn.onClick.RemoveAllListeners();
    }

    private void Unit_Selected(object sender, System.EventArgs e)
    {
        //btn.GetComponent<Image>().sprite = sprt_move;
        _btn.onClick.AddListener(() => OnBtnClick());
    }

    private void OnBtnClick()
    {
        _unit.Is_clicked = true;
    }
 
}
