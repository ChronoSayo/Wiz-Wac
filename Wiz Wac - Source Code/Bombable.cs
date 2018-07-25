using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bombable : MonoBehaviour
{
    void OnTriggerEnter(Collider col)
    {
        if (col.transform.name == "Explosion")
        {
            GetComponent<Rigidbody>().useGravity = true;
            GetComponent<Rigidbody>().isKinematic = false;
            transform.parent = null;
            StartCoroutine(RemoveDebris());
        }
    }

    private IEnumerator RemoveDebris()
    {
        yield return new WaitForSeconds(10);
        Destroy(gameObject);
    }
}
