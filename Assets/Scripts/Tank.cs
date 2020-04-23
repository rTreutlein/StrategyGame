using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{
    [Header ("Attributes")]
    public float fire_range = 5f;
    public float fire_rate = 1f;

    [Header ("Unity Stuff")]
    public Transform turret;
    public GameObject bullet;
    public Transform fire_point;

    private Transform target;

    private float fireCountdown = 0f;
    private bool attack_mode;
  
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null)
            return;

        Vector3 dir = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = Quaternion.Lerp(turret.rotation, lookRotation, Time.deltaTime * 3).eulerAngles;
        turret.rotation = Quaternion.Euler(0f, rotation.y, 0f);

        if (fireCountdown <= 0f && UnityEngine.Random.value > 0.3)
        {
            Shoot(dir);
            fireCountdown = 1f / fire_rate;
        }

        fireCountdown -= Time.deltaTime;

    }

    private void Shoot(Vector3 dir)
    {
        Debug.Log("Shoot");
        GameObject b = Instantiate(bullet, fire_point.position, fire_point.rotation);
        b.GetComponent<Rigidbody>().AddForce((dir * 3) + (Vector3.up * 1.2f), ForceMode.VelocityChange);
    }

    private void OnTriggerEnter(Collider other)
    {
        Unit unit = GetComponent<Unit>();
        Unit ounit = other.GetComponent<Unit>();
        if (ounit == null)
            return;
        if (ounit.team == unit.team)
            return;
        if (target == null)
            target = other.transform;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform == target && !attack_mode)
            target = null;
    }
}
