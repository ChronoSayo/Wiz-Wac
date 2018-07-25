using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{
    public Image playerIcon;
    
    private Transform _player;

    void Start ()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void OnPreRender()
    {
        RenderSettings.fog = false;
    }

    void OnPostRender()
    {
        RenderSettings.fog = true;
    }

    void Update ()
    {
        if(!_player.GetComponent<Link>().IsDead)
            CameraMovement();
        PlayerIconMovement();
    }

    private void PlayerIconMovement()
    {
        playerIcon.rectTransform.rotation = Quaternion.Euler(90, 0, -_player.rotation.eulerAngles.y);
    }

    private void CameraMovement()
    {
        transform.LookAt(_player);
        transform.position = new Vector3(_player.position.x, _player.position.y + 300, _player.position.z);
        transform.rotation = Quaternion.Euler(90, 0, 0);
    }
}
