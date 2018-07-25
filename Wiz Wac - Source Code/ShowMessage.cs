using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowMessage : MonoBehaviour
{
    public static ShowMessage GetInstance;

    private Text _message;
    private State _state;

    private enum State
    {
        None, Show, End
    }

    void Awake()
    {
        GetInstance = this;
    }

    void Start()
    {
        _message = GetComponent<Text>();
    }

    void Update()
    {
        switch(_state)
        {
            case State.None:
                break;
            case State.Show:
                Fade(true);
                break;
            case State.End:
                Fade(false);
                break;
        }
    }

    public void PlayMessage(string message, float time)
    {
        _state = State.Show;
        _message.text = message;

        Color c = _message.color;
        _message.color = new Color(c.r, c.g, c.b, 0);

        StartCoroutine(EndMessage(time + 3));
    }

    private IEnumerator EndMessage(float sec)
    {
        yield return new WaitForSeconds(sec);
        _state = State.End;
    }

    private void Fade(bool show)
    {
        float speed = 0.0025f;
        if (!show)
            speed *= -1;

        _message.color += new Color(0, 0, 0, speed);

        Color c = _message.color;
        c.a = Mathf.Clamp(c.a, 0, 1);
        _message.color = c;

        if (_message.color.a <= 0)
            _state = State.End;
    }
}
