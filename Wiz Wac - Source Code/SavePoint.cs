using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint : MonoBehaviour
{
    private Vector3 _restartPosition, _restartRotation;

    void Start()
    {
        _restartPosition = transform.GetChild(0).position;
        _restartRotation = transform.GetChild(0).rotation.eulerAngles;
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.transform.tag == "Player")
        {
            col.transform.GetComponent<Link>().StartPosition = _restartPosition;
            col.transform.GetComponent<Link>().StartRotation = _restartRotation;
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player")
        {
            col.transform.GetComponent<Link>().StartPositionOverworld = _restartPosition;
            col.transform.GetComponent<Link>().StartRotation = _restartRotation;
        }
    }
}
