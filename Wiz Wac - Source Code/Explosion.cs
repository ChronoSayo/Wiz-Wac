using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    void OnTriggerEnter(Collider col)
    {
        Rigidbody rb = col.GetComponent<Rigidbody>();
        if (rb)
            StartCoroutine(AddExplosionForce(rb));
    }

    private IEnumerator AddExplosionForce(Rigidbody rb)
    {
        yield return new WaitForFixedUpdate();
        rb.AddExplosionForce(500, transform.position, transform.localScale.magnitude, 0, ForceMode.Impulse);
    }
}
