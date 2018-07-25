using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlatform : MonoBehaviour
{
    public float speed, waitTime;
    public int startNode = 1;

    private SwitchLeader _switchLeader;
    private int _currentNode;
    private float _speed;
    private bool _stop;
    private Transform _platform;
    private List<Vector3> _nodes;

    void Start ()
    {
        _nodes = new List<Vector3>();
        foreach (Transform t in transform)
        {
            if (t.name == "Platform")
                _platform = t;
            else
                _nodes.Add(t.position);
        }

        if (transform.parent)
            _switchLeader = transform.parent.GetComponent<SwitchLeader>();

        _stop = false;
        _speed = speed;
        _currentNode = startNode - 1;
    }
    
    void Update ()
    {
        if(Vector3.Distance(_platform.position, _nodes[_currentNode]) < 0.1f && !_stop)
        {
            _stop = true;
            StartCoroutine(StopPlatform());
        }
    }
    
    void FixedUpdate()
    {
        if (((_switchLeader && !_switchLeader.off) || !_switchLeader) && !_stop)
            _speed = speed;
        else
            _speed = 0;

        _platform.position += (_nodes[_currentNode] - _platform.position).normalized * _speed * Time.deltaTime;
    }

    private IEnumerator StopPlatform()
    {
        yield return new WaitForSeconds(waitTime);
        _stop = false;

        _currentNode++;
        if (_currentNode >= _nodes.Count)
            _currentNode = 0;

    }

    public Vector3 NextPosition
    {
        get { return _nodes[_currentNode]; }
    }

    public float GetSpeed
    {
        get { return _speed; }
    }
}
