using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockBuff : MonoBehaviour
{
    public BuffList buff;

    private Buff _buffScript;
    private Transform _buffIcon;

    public enum BuffList
    {
        Speed, Jump, Climb, Fuel
    }

    void Start()
    {
        _buffScript = GameObject.FindGameObjectWithTag("Player").GetComponent<Buff>();

        foreach (Transform t in transform)
        {
            if (!t.name.Contains("Cube"))
                _buffIcon = t;
        }
    }

    void Update()
    {
        _buffIcon.Rotate(0, 1, 0);
    }

    public void SetBuff()
    {
        _buffScript.ResetBuff();
        switch(buff)
        {
            case BuffList.Speed:
                _buffScript.EnableBuffSpeed(true);
                break;
            case BuffList.Jump:
                _buffScript.EnableBuffJump(true);
                break;
            case BuffList.Climb:
                _buffScript.EnableBuffClimb(true);
                break;
            case BuffList.Fuel:
                _buffScript.EnableBuffFuel(true);
                break;
        }
    }
}
