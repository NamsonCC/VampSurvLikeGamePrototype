using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance;
    public delegate void PlayAudioDelegate(int a);
    public static PlayAudioDelegate PlayAudio_Delegate;

    public AudioSource audioPrefab;
    private Dictionary<int, List<AudioSource>> sourcesDicitonary = new Dictionary<int, List<AudioSource>>();
    private Dictionary<int, Queue<AudioSource>> sourcesQueu = new Dictionary<int, Queue<AudioSource>>();
    public enum SoundTypes
    {
        ambiance,
        hit,
        levelup,
        powerup,
        collectXP,
        collectItem,
    }
    private void Awake()
    {
        Instance = this;
        PlayAudio_Delegate = null;
        PlayAudio_Delegate += PlayAudio;


        var array = Enum.GetValues(typeof(SoundTypes));
        for (int i = 0; i < array.Length; i++)
        {
            sourcesDicitonary.Add(i, new List<AudioSource>());
            sourcesQueu.Add(i, new Queue<AudioSource>());
        }
    }

    [SerializeField] private AudioSource[] SoundSources;

    private void PlayAudio(int whichAudioSource)
    {
        PlaySound(whichAudioSource, SoundSources[whichAudioSource].clip, SoundSources[whichAudioSource].pitch, SoundSources[whichAudioSource].volume);
    }



    public void PlaySound(int whichAudioSource, AudioClip clip, float pitch, float volume = 1f)
    {
        AudioSource source = GetAvailableSource(whichAudioSource);
        sourcesQueu[whichAudioSource].Enqueue(source);
        source.clip = clip;
        source.pitch = pitch;
        source.volume = volume;
        source.Play();
        StartCoroutine(ReleaseWhenDone(whichAudioSource, source));
    }

    private AudioSource GetAvailableSource(int whichAudioSource)
    {
        foreach (var src in sourcesDicitonary[whichAudioSource])
        {
            if (!src.isPlaying)
                return src;
        }
        if (sourcesDicitonary[whichAudioSource].Count <= 5)
        {
            var newSource = Instantiate(audioPrefab, transform);
            sourcesDicitonary[whichAudioSource].Add(newSource);
            return newSource;
        }
        else
        {
            AudioSource source =sourcesQueu[whichAudioSource].Dequeue();
            return source;
        }

    }

    private System.Collections.IEnumerator ReleaseWhenDone(int whichAudioSource, AudioSource source)
    {
        yield return new WaitForSeconds(source.clip.length);
        yield return new WaitWhile(() => source.isPlaying);
        sourcesQueu[whichAudioSource].TryDequeue(out AudioSource audioSource);
        source.clip = null;
    }
}
