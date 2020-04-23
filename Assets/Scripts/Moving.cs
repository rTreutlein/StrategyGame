using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Panda;

public class Moving : MonoBehaviour
{
    public Sprite sprt_move;
    public Sprite sprt_stop;
    public Sprite sprt_default;

    public Texture2D tex_mov_cursor;
    public Texture2D tex_def_cursor;

    private UnityEngine.AI.NavMeshAgent agent;

    private Unit unit;
    private Button btn_mov;
    private Button btn_stop;

    private Quaternion? _dir = null;

    // Start is called before the first frame update
    void Awake()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        unit = GetComponent<Unit>();
        unit.OnSelected += Unit_Selected;
        unit.OnDeselected += Unit_Deselected;

        Transform btns = GameObject.FindObjectOfType<InputManager>().ui.transform.Find("btns");
        btn_mov = btns.Find("btn1x1").GetComponent<Button>();
        btn_stop = btns.Find("btn1x3").GetComponent<Button>();
    }

    void Update()
    {
        RightClick();
        LeftClick();

        if (IsMoving())
            return;
        if (_dir.HasValue)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, _dir.Value, Time.deltaTime);
        }
    }

    [Task]
    public bool IsMoving()
    {
        if (agent.pathPending)
            return true;
        if (agent.remainingDistance > agent.stoppingDistance)
            return true;
        if (agent.hasPath || agent.velocity.sqrMagnitude != 0f)
            return true;
        return false;
    }

    private void Unit_Deselected(object sender, System.EventArgs e)
    {
        btn_mov.GetComponent<Image>().sprite = sprt_default;
        btn_stop.GetComponent<Image>().sprite = sprt_default;
        btn_mov.onClick.RemoveAllListeners();
        btn_stop.onClick.RemoveAllListeners();
    }

    private void Unit_Selected(object sender, System.EventArgs e)
    {
        btn_mov.GetComponent<Image>().sprite = sprt_move;
        btn_stop.GetComponent<Image>().sprite = sprt_stop;
        btn_mov.onClick.AddListener(() => OnMoveClick());
        btn_stop.onClick.AddListener(() => OnStopClick());
    }

    private void OnStopClick()
    {
        agent.destination = transform.position;
    }

    private void OnMoveClick()
    {
        unit.Is_clicked = true;
        Cursor.SetCursor(tex_mov_cursor, new Vector2(75, 45), CursorMode.Auto);
    }

    private void LeftClick()
    {
        if (unit.Is_clicked && Input.GetMouseButtonDown(0))
        {
            MoveToMouse();
            unit.Is_clicked = false;
            Cursor.SetCursor(tex_def_cursor, new Vector2(75, 45), CursorMode.Auto);
        }
    }

    private void RightClick()
    {
        if(unit.Is_selected && Input.GetMouseButtonDown(1))
        {
            if (unit.Is_clicked)
            {
                unit.Is_clicked = false;
                Cursor.SetCursor(tex_def_cursor, new Vector2(75, 45), CursorMode.Auto);
                return;
            }

            MoveToMouse();
        }
    }

    private void MoveToMouse()
    {
        Debug.Log("Moving");

        RaycastHit hitInfo;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
        {
            if (hitInfo.collider.tag == "Ground")
                agent.destination = hitInfo.point;
            else if (hitInfo.collider.tag == "Mine" || hitInfo.collider.tag == "Player")
                agent.destination = hitInfo.transform.position;
        }
    }

    public void SetDestination(Vector3 dest, Quaternion? dir = null)
    {
        Debug.Log("Dir is: " + dir?.eulerAngles);
        agent.destination = dest;
        _dir = dir;
    }

}
