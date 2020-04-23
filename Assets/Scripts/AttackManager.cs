using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;

public class AttackManager : MonoBehaviour
{
    public Transform _start_tf;
    public float _ratio;
    public float _max_dist;

    public List<Moving> _units = new List<Moving>();

    private Vector3 _pos;
    private Vector3 _dir;

    private String _current_action;

    // Start is called before the first frame update
    void Awake()
    {
        _pos = _start_tf.position;
        _dir = _start_tf.rotation.eulerAngles;
    }

    public void AddUnit(Moving unit)
    {
        _units.Add(unit);
        GetFormationPoints(_pos, _dir);
    }

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(_pos, 0.3f);
    }

    public void GetFormationPoints(Vector3 start, Vector3 dir)
    {
        int unit_count = _units.Count;
        if (unit_count == 0)
            return;

        _pos = start;
        _dir = dir;

        int width = Mathf.RoundToInt(Mathf.Sqrt(unit_count * _ratio));
        int depth = Mathf.RoundToInt(Mathf.Sqrt(unit_count / _ratio));

        Debug.Log("Foration Widht: " + width);
        Debug.Log("Foration Depth: " + depth);

        //Count invalid posions
        int index_offset = 0;
        dir.y = 0;
        dir.Normalize();
        dir *= 1.2f;
        Vector3 dirside = Quaternion.Euler(0, 90, 0) * dir;
        Vector3 offset = Quaternion.Euler(dir) * new Vector3(-width / 2, 0, depth / 2);

        Debug.Log("offset" + offset);

        Vector3? last_pos = null;
        for (int i = 0; true; i++)
        {
            offset -= dirside;
            for (int j = 0; j < width; j++)
            {
                offset += dirside;
                int idx = i * width + j - index_offset;
                if (unit_count <= idx || index_offset > unit_count)
                    return;

                Vector3 pos = start + offset;
                if (Physics.Raycast(pos + (Vector3.up * 5), Vector3.down, out RaycastHit hit, 10))
                {
                    if (hit.transform.tag != "Ground")
                    {
                        index_offset++; continue;
                    }
                    if (last_pos == null || (hit.point - last_pos).GetValueOrDefault().magnitude < _max_dist)
                    {
                        _units[idx].SetDestination(hit.point, Quaternion.Euler(dir));
                        last_pos = hit.point;
                    }
                    else
                    {
                        index_offset++; continue;
                    }
                }
                else
                {
                    index_offset++; continue;
                }

            }
            offset -= dir;
            dirside *= -1;
        }
    }


    [Task]
    public bool Defend(int dist)
    {
        GameObject[] cores = GameObject.FindGameObjectsWithTag("Player");
        GameObject enemy_core = null;
        foreach (GameObject core in cores)
        {
            if (core.GetComponent<Unit>().team != GetComponent<Unit>().team)
                enemy_core = core;
        }
        if (enemy_core == null)
            return false;

        Vector3 dir = (enemy_core.transform.position - transform.position).normalized;
        Vector3 pos = transform.position + dir * dist;
        GetFormationPoints(pos, dir);
        _current_action = "Defending";
        return true;
    }

    [Task]
    public bool AtkIs(String what)
    {
        return _current_action == what;
    }

}
