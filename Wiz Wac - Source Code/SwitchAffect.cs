using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchAffect : MonoBehaviour
{
    public Vector3 onPosition;
    public bool relativePosition;
    public float speed;

    private SwitchLeader _switchLeader;
    private Vector3 _offPosition;
    private bool _boss;

    void Start ()
    {
        _switchLeader = transform.parent.GetComponent<SwitchLeader>();

        //Just for the boss.
        if (!_switchLeader)
            _switchLeader = GameObject.Find("SwitchLeader Special").GetComponent<SwitchLeader>();

        _offPosition = transform.position;

        _boss = GameplayHandler.IS_BOSS;
    }

    void Update()
    {
        if (_boss)
            Hide();
    }

    private void Hide()
    {
        GetComponent<Collider>().enabled = _switchLeader.off;
        GetComponent<Renderer>().enabled = _switchLeader.off;
    }
    
    void FixedUpdate ()
    {
        if (_boss)
            return;
        Vector3 pos = _switchLeader.off ? _offPosition : relativePosition ? _offPosition + onPosition : onPosition;

        if(Vector3.Distance(transform.position, pos) > 1)
            transform.position += (pos - transform.position).normalized * speed * Time.deltaTime;
    }
}
