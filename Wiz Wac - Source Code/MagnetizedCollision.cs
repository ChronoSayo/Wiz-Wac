using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetizedCollision : MonoBehaviour
{
    private ContactPoint[] _contactPoints;

    void OnCollisionStay(Collision col)
    {
        _contactPoints = col.contacts;
    }

    void OnCollisionExit(Collision col)
    {
        _contactPoints = null;
    }

    public ContactPoint[] ContactPoints
    {
        get { return _contactPoints; }
    }
}
