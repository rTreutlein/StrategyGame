using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        Unit ounit = collision.gameObject.GetComponent<Unit>();

        if (ounit)
            ounit.Damage(10);

        Destroy(gameObject);
    }
}
