using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio : MonoBehaviour
{
    public AudioSource intro;
    public AudioSource main;
    public AudioSource hit;
    public AudioSource roll;
    public AudioSource death;
    public AudioSource levelup;

    void Start()
    {
        intro.Play();
        StartCoroutine("MusicCoroutine");
    }

    public void HitAudio()
    {
        hit.Play();
    }

    public void Roll()
    {
        roll.Play();
    }

    public void Levelup()
    {
        levelup.Play();
    }

    public void Death()
    {
        if (intro.isPlaying)
        {
            intro.Stop();
        }
        if (main.isPlaying)
        {
            main.Stop();
        }
        death.Play();
    }

    IEnumerator MusicCoroutine()
    {
        while(intro.isPlaying)
        {
            yield return null;
        }
        if (!death.isPlaying)
        {
            main.Play();
        }
    }
}
