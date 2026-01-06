using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomMusicPlayer : MonoBehaviour
{
    [Header("Playlist")]
    [Tooltip("Liste des musiques à jouer aléatoirement.")]
    public List<AudioClip> tracks = new List<AudioClip>();

    [Header("Settings")]
    public bool playOnStart = true;
    public bool loopPlaylist = true;
    public bool avoidImmediateRepeat = true;
    [Range(0f, 1f)] public float volume = 1f;

    private AudioSource _source;
    private int _lastIndex = -1;

    void Awake()
    {
        _source = GetComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.loop = false; // On gère le passage au prochain nous-mêmes
    }

    void Start()
    {
        _source.volume = volume;

        if (playOnStart)
            PlayRandomTrack();
    }

    void Update()
    {
        // Si rien ne joue et qu'on a une playlist, on enchaîne
        if (loopPlaylist && tracks != null && tracks.Count > 0 && !_source.isPlaying && _source.clip != null)
        {
            PlayRandomTrack();
        }
    }

    public void PlayRandomTrack()
    {
        if (tracks == null || tracks.Count == 0)
        {
            Debug.LogWarning("[RandomMusicPlayer] Aucune musique dans tracks.");
            return;
        }

        int index = GetRandomIndex(tracks.Count);

        _source.clip = tracks[index];
        _source.volume = volume;
        _source.Play();

        _lastIndex = index;
    }

    public void StopMusic()
    {
        _source.Stop();
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        _source.volume = volume;
    }

    private int GetRandomIndex(int count)
    {
        if (count == 1) return 0;

        int index = Random.Range(0, count);

        if (avoidImmediateRepeat && index == _lastIndex)
        {
            // Reroll
            index = Random.Range(0, count);

            // Un fallback au cas où on retombe sur le même
            if (index == _lastIndex)
                index = (index + 1) % count;
        }

        return index;
    }
}
