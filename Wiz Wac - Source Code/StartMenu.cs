using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    private Text _text;
    private bool _darkText;

    static public bool USE_KEYBOARD = false;

    void Start ()
    {
        _text = GetComponent<Text>();
        _darkText = false;
    }
    
    void Update ()
    {
        PlayerInput.SimulateKeyPress();

        Blink();
        PressStart();
    }
    
    private void Blink()
    {
        float speed = 0.0025f;
        if (!_darkText)
            speed *= -1;

        _text.color += new Color(0, 0, 0, speed);

        Color c = _text.color;
        c.a = Mathf.Clamp(c.a, 0, 1);
        _text.color = c;

        if (c.a <= 0)
            _darkText = true;
        if (c.a >= 1)
            _darkText = false;
    }

    private void PressStart()
    {
        bool enter = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
        if (Input.GetButton("StartGame") || enter)
            StartGame(enter);
    }

    private void StartGame(bool useKeyboard)
    {
        USE_KEYBOARD = useKeyboard;
        _text.text = "LOADING";
        _text.color = new Color(1, 0, 0, 1);
        SceneManager.LoadScene(GameplayHandler.OVERWORLDLEVEL);
    }
}
