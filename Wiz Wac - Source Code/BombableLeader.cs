using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombableLeader : MonoBehaviour
{
    void OnTriggerEnter(Collider col)
    {
        if (col.transform.name == "Explosion")
        {
            Destroy(gameObject);
        }
    }
}
