using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Outro : MonoBehaviour
{
    private Text _message, _loading;
    private Vector3 _focusPoint;
    private float _showTextTime, _showTextTick, _fadeTime, _fadeTick;
    private Color _skyColor;

    void Start ()
    {
        _focusPoint = GameObject.Find("Carnon's Platform").transform.position + (Vector3.up * 120);
        _loading = GameObject.Find("EndLoading").GetComponent<Text>();
        _loading.gameObject.SetActive(false);

        _message = GameObject.Find("EndMessage").GetComponent<Text>();

        _fadeTick = _showTextTick = 0;
        _showTextTime = 1.3f;
        _fadeTime = 0.25f;

        _skyColor = Color.black;
    }
    
    void Update ()
    {
        transform.LookAt(_focusPoint);

        bool slowMotion = Time.timeScale < 1;
        Rotation(slowMotion);

        HandleText();

        SlowMotion(slowMotion);
    }

    private void Rotation(bool slowMotion)
    {
        float rot = -5;
        if (slowMotion)
            rot = -50;
        transform.RotateAround(_focusPoint, Vector3.up, rot * Time.deltaTime);
    }

    private void HandleText()
    {
        _showTextTick += Time.deltaTime;
        if (_showTextTick >= _showTextTime)
        {
            _fadeTick += Time.deltaTime;
            Fade(_fadeTick <= _fadeTime);
        }
    }

    private void SlowMotion(bool slowMotion)
    {
        if (slowMotion)
        {
            float colorSpeed = 0.00025f;
            _skyColor += new Color(colorSpeed, colorSpeed, colorSpeed);
            RenderSettings.skybox.SetColor("_Tint", _skyColor);
        }
    }

    private void Fade(bool show)
    {
        float speed = 0.001f;
        if (!show)
            speed *= -1;
        
        _message.color += new Color(0, 0, 0, speed);

        Color c = _message.color;
        c.a = Mathf.Clamp(c.a, 0, 1);
        _message.color = c;

        if (_message.color.a <= 0)
        {
            _loading.gameObject.SetActive(true);
            Time.timeScale = 1;
            RenderSettings.skybox.SetColor("_Tint", Color.black);
            SceneManager.LoadScene(GameplayHandler.OVERWORLDLEVEL);
        }
    }
}
