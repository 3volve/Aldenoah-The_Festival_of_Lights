using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    private static readonly float TIME_TO_START_PLAYING_SONG_EARLY = 0.45f;

    #pragma warning disable

    [SerializeField] AudioMixer mixer;
    [SerializeField] AudioClip[] music = new AudioClip[3];
    [SerializeField] AudioClip[] transitions;
    [SerializeField] float[] transitionChangeovers1 = new float[4];
    [SerializeField] float[] transitionChangeovers2 = new float[4];
    [SerializeField] string[] volumeNames = new string[2];
    [SerializeField] Slider[] volumeSliders = new Slider[2];

    #pragma warning restore

    List<float[]> transitionChangeoverTimes;
    AudioSource source;
    int curSongIndex = 0;

    void Awake()
    {
        transitionChangeoverTimes = new List<float[]>
            {
                transitionChangeovers1,
                transitionChangeovers2
            };

        source = GetComponent<AudioSource>();
        StartTransition(0);
    }

    public void DEBUG_SetMusic(int songIndex)
    {
        source.clip = music[songIndex];
        source.Play();
        curSongIndex = songIndex;
    }

    public void StartTransition(int songIndex)
    {
        curSongIndex = songIndex;
        
        if (songIndex > 0)
        {
            int changeoverIndex = -1;

            for (int i = 0; i < transitionChangeoverTimes[songIndex - 1].Length; i++)
                if (source.time + 1 <= transitionChangeoverTimes[songIndex - 1][i])
                {
                    changeoverIndex = i;
                    break;
                }

            float transitionStartTime = source.clip.length;

            if (changeoverIndex != -1)
                transitionStartTime = transitionChangeoverTimes[songIndex - 1][changeoverIndex];

            float timeToStartTransition = transitionStartTime - source.time;

            StartCoroutine(PlayCurTransition(transitionStartTime));
        }
        else StartCoroutine(PlayCurTransition(0));
    }

    IEnumerator PlayCurTransition(float timeToStartPlaying)
    {
        yield return new WaitUntil(() => Mathf.Abs(source.time - timeToStartPlaying) < 0.1f);

        source.Stop();
        source.PlayOneShot(transitions[curSongIndex]);

        if (curSongIndex < music.Length)
        {
            source.clip = music[curSongIndex];
            source.PlayDelayed(transitions[curSongIndex].length - TIME_TO_START_PLAYING_SONG_EARLY);
        }

        yield break;
    }

    public void OnMusicVolumeChanged(int sliderIndex)
        => mixer.SetFloat(volumeNames[sliderIndex], volumeSliders[sliderIndex].value - 60);
}
