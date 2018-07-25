using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioComponent : MonoBehaviour
{
    public AudioClip intro;
    [Tooltip("0: Plateau Mountain, 1: Mountain, 2: Temple, 3: Labyrinth, 4: Canyon, 5: Star")]
    public List<Transform> areaSongs;
    [Tooltip("0: Plateau Mountain, 1: Mountain, 2: Temple, 3: Labyrinth, 4: Canyon, 5: Star")]
    public List<AudioClip> clips;

    public static AudioComponent GetInstance;

    private AudioSource _intro;
    private Transform _player;
    private float _fadeSpeed;
    ///<summary>
    ///0: Plateau Mountain, 1: Mountain, 2: Temple, 3: Labyrinth, 4: Canyon, 5: Star
    ///</summary>
    private List<AudioSource> _areaSongs;

    void Awake()
    {
        GetInstance = this;
    }

    void Start ()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        
        if(GameplayHandler.IS_OVERWORLD)
        {
            _intro = gameObject.GetComponent<AudioSource>();
            _intro.clip = intro;

            _areaSongs = new List<AudioSource>();
            foreach (AudioClip c in clips)
                CreateAudioSource(c);
        }

        _fadeSpeed = 0.0005f;
    }

    private void CreateAudioSource(AudioClip clip)
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = 0;
        _areaSongs.Add(source);
    }

    void Update()
    {
        if (GameplayHandler.IS_OVERWORLD)
        {
            for (int i = 0; i < _areaSongs.Count; i++)
                CheckSongArea(areaSongs[i], _areaSongs[i]);
        }
    }

    private void CheckSongArea(Transform area, AudioSource source)
    {
        if (Vector3.Distance(_player.position, area.position) < area.GetComponent<SongArea>().startDistance)
        {
            if (!source.isPlaying)
                PlaySong(source);
            else
                Fade(true, source);
        }
        else
        {
            if (source.isPlaying)
                Fade(false, source);
        }
    }
    
    public void PlayIntro()
    {
        _intro.Play();
        _intro.volume = 1;
    }

    private void PlaySong(AudioSource source)
    {
        source.volume = 0;
        source.Play();
    }

    public void StopSong()
    {
        _intro.Stop();
    }

    public bool SongPlaying
    {
        get { return _intro.isPlaying; }
    }

    public void Fade(bool start, AudioSource source)
    {
        source.volume += start ? _fadeSpeed : -_fadeSpeed;
        bool full = start && source.volume >= 1;
        bool deaf = !start && source.volume <= 0;
        if (full || deaf)
        {
            float minMax = start ? 1 : 0;
            source.volume = minMax;
            if (!start)
                source.Stop();
        }
    }
}
