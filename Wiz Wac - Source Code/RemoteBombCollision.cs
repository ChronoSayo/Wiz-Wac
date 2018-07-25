using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteBombCollision : MonoBehaviour
{
    void OnCollisionStay(Collision col)
    {
        if (col.transform.name == "Platform")
        {
            StartCoroutine(MoveWithPlatform(col.transform));
        }
    }

    private IEnumerator MoveWithPlatform(Transform platform)
    {
        yield return new WaitForFixedUpdate();
        float speed = platform.parent.GetComponent<MovePlatform>().speed;
        transform.position += platform.forward * speed * Time.deltaTime;
    }
}
