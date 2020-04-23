using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    public float pan_speed;
    public float rotateSpeed;
    public float rotateAmount;

    public GameObject ui;

    private GameObject selected_obj;
    private ISelectable selected;
    private Quaternion rotation;

    private float pan_detect = 15f;
    public float min_height = 5f;
    public float max_height = 50f;

    // Start is called before the first frame update
    void Start()
    {
        rotation = Camera.main.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        MoveCamera();
        RotateCamera();
        SelectObject();

        if (Input.GetKeyDown(KeyCode.Space))
            Camera.main.transform.rotation = rotation;
    }

    private void SelectObject()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        
        if (!Input.GetMouseButtonDown(0))
            return;

        RaycastHit hitInfo = new RaycastHit();
        bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 100f);
        if (!hit) return;

        Debug.Log(hitInfo.collider.tag);

        if (hitInfo.collider.tag == "Ground")
        {
            Deselect();
        }
        else
        {
            Debug.Log("Selected");
            if (selected_obj != hitInfo.collider.gameObject)
                Deselect();

            selected_obj = hitInfo.collider.gameObject;

            selected = selected_obj.GetComponent<ISelectable>();
            if (selected != null)
                selected.Select();
        }
    }

    private void Deselect()
    {
        if (selected != null)
            if (!selected.Deselect())
                return;

        Debug.Log("Deselected");
        selected_obj = null;
        selected = null;
    }

    private void MoveCamera()
    {
        float move_x = Camera.main.transform.position.x;
        float move_y = Camera.main.transform.position.y;
        float move_z = Camera.main.transform.position.z;

        float xPos = Input.mousePosition.x;
        float yPos = Input.mousePosition.y;

        if(Input.GetKey(KeyCode.A) || (xPos > 0 && xPos < pan_detect))
        {
            move_x -= pan_speed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.D) || (xPos < Screen.width && xPos >  Screen.width - pan_detect))
        {
            move_x += pan_speed * Time.deltaTime;
        }

        if(Input.GetKey(KeyCode.S) || (yPos > 0 && yPos < pan_detect))
        {
            move_z -= pan_speed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.W) || (yPos < Screen.width && yPos >  Screen.width - pan_detect))
        {
            move_z += pan_speed * Time.deltaTime;
        }

        move_y -= Input.GetAxis("Mouse ScrollWheel") * (pan_speed * 20) * Time.deltaTime;
        move_y = Mathf.Clamp(move_y, min_height, max_height);

        Vector3 new_pos = new Vector3(move_x, move_y, move_z);

        Camera.main.transform.position = new_pos;
    }

    private void RotateCamera()
    {
        Vector3 origin = Camera.main.transform.eulerAngles;
        Vector3 destination = origin;

        if(Input.GetMouseButton(2))
        {
            destination.x -= Input.GetAxis("Mouse Y") * rotateAmount;
            destination.y += Input.GetAxis("Mouse X") * rotateAmount;
        }

        if(destination != origin)
        {
            Camera.main.transform.eulerAngles = Vector3.MoveTowards(origin, destination, Time.deltaTime * rotateSpeed);
        }
    }
}
