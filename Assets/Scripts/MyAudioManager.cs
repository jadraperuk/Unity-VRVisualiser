using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Audio;

public class MyAudioManager : MonoBehaviour {

    public Sound[] sounds;
	// Use this for initialization
	void Awake () {
		foreach (Sound S in sounds)
        {
            S.source = gameObject.AddComponent<AudioSource>();
            S.source.clip = S.clip;
            S.source.playOnAwake = false;
            S.source.volume = S.volume;
            S.source.pitch = S.pitch;
            S.source.loop = S.loop;
            S.source.minDistance = 0;
            S.source.rolloffMode = AudioRolloffMode.Linear;
        }
	}

    private void Start()
    {
        Play("BackgroundMusic");
    }

    public void Play(string Name)
    {
        Sound s = Array.Find(sounds, sound => sound.clipname == Name);
        if (s == null)
            return;        
        if (!s.source.isPlaying)
        {
            s.source.Play();
            Debug.Log("playing sound: " + Name);
        }
    }

    public void Stop(string Name)
    {
        Sound s = Array.Find(sounds, sound => sound.clipname == Name);
        if (s == null)
            return;
        s.source.Stop();
    }
}
