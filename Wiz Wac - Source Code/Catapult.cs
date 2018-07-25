using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Catapult : MonoBehaviour
{
    public float shootCooldown, offset,  power, powerDecrease;
    private Vector3 _startPos;
    private State _state;
    private float _power, _speed;

    private enum State
    {
        None, Shoot, Charge, Retract
    }

    void Start ()
    {
        _startPos = transform.position;
        _state = State.Shoot;
        _speed = 50;
        _power = power;
    }

    void Update()
    {
        switch(_state)
        {
            case State.None:
                break;
            case State.Shoot:
                StartCoroutine(ShootMotion());
                break;
            case State.Charge:
                break;
            case State.Retract:
                StartCoroutine(RetractMotion());
                break;
        }
    }
    
    private IEnumerator ShootMotion()
    {
        yield return new WaitForFixedUpdate();
        if (transform.position.y <= _startPos.y + offset)
            transform.position += transform.forward * _speed * Time.deltaTime;
        else
        {
            _state = State.Charge;
            StartCoroutine(DelayRetract());
        }
    }

    private IEnumerator DelayShoot()
    {
        yield return new WaitForSeconds(shootCooldown);
        _state = State.Shoot;
    }

    private IEnumerator DelayRetract()
    {
        yield return new WaitForSeconds(1);
        _state = State.Retract;
    }
    
    private IEnumerator RetractMotion()
    {
        yield return new WaitForFixedUpdate();
        if (transform.position.y >= _startPos.y)
            transform.position -= transform.forward * (_speed / 2) * Time.deltaTime;
        else
        {
            transform.position = _startPos;
            _power = power;
            _state = State.Charge;
            StartCoroutine(DelayShoot());
        }
    }

    private IEnumerator ShootCollision(Collision col)
    {
        yield return new WaitForFixedUpdate();
        col.transform.GetComponent<Rigidbody>().AddForce(transform.forward * _power, ForceMode.Impulse);
        _power -= powerDecrease;
    }

    void OnCollisionStay(Collision col)
    {
        if (col.transform.GetComponent<Rigidbody>() && _state == State.Shoot)
        {
            StartCoroutine(ShootCollision(col));
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.transform.GetComponent<Rigidbody>() && _state == State.Shoot)
        {
            col.transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
            col.transform.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }
    }
}
