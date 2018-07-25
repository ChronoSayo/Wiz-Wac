using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagneticReset : MonoBehaviour
{
    public Vector3 startPosition;
    public float deathY;

    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private bool _boss;

    void Start ()
    {
        _startPosition = startPosition == Vector3.zero ? transform.position : startPosition;
        _startRotation = transform.rotation;

        _boss = GameplayHandler.IS_BOSS;
    }
    
    void Update ()
    {
        if (transform.position.y <= -deathY)
        {
            if (_boss)
                Destroy(gameObject);
            else
            {
                transform.position = _startPosition;
                transform.rotation = _startRotation;

                Rigidbody rb = GetComponent<Rigidbody>();
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}
