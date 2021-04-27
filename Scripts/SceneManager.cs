using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{
#pragma warning disable
    [SerializeField] RectTransform introScreen;
    [SerializeField] float timeForIntroToFade;
    [SerializeField] float timeForIntroTextToFade;

    [SerializeField] RectTransform titleScreen;
    [SerializeField] float timeForTitleToFade;
    [SerializeField] float timeForTitleToTransition;
    [SerializeField] Transform startingInGameCameraLocation;

    [SerializeField] RectTransform endScreen;
    [SerializeField] TMP_Text[] creditTexts = new TMP_Text[5];
    [SerializeField] float timeForEndToFade;
    [SerializeField] float timeForEndTextToFade;
    [SerializeField] float timeForEndCreditsToFade;
    [SerializeField] Button endButton;

    [SerializeField] RectTransform settingsScreen;
    [SerializeField] RectTransform settingsButton;
    [SerializeField] Vector2 inGameSettingsButtonPosition;
    [SerializeField] Vector3 openSettingsScale;
    [SerializeField] Vector3 closedSettingsScale;
    [SerializeField] Vector2 openSettingsPosition;
    [SerializeField] float timeForSettingsToOpen;

    [SerializeField] PlayerController1 player;
    [SerializeField] BoxCollider introCharacterCollider;
    [SerializeField] AudioMixer mixer;
    [SerializeField] AudioManager audioManager;
#pragma warning restore

    public int AdeptsCount { get; private set; } = 4;

    bool finishedIntro = false;
    bool startedGame = false;
    bool playingGame = false;
    bool endingGame = false;
    bool gameEnd = false;
    int endingStep = 0;
    bool settingsTransition = false;

    float startingPlayTransitionTime;
    float startingEndTransitionTime;
    float startingSettingsTransitionTime;

    Vector3 startingTitlePosition = Vector3.zero;
    Quaternion startingTitleRotation = Quaternion.identity;
    
    Vector2 startingSettingsButtonPosition;
    bool settingsIsOpen = false;

    AudioSource source;
    float currentTime;

    void Start()
    {
        source = GetComponent<AudioSource>();
        mixer.SetFloat("FootstepVolume", -80);

        startingTitlePosition = Camera.main.transform.position;
        startingTitleRotation = Camera.main.transform.rotation;

        currentTime = Time.time;

        Vector3 settingsButtonPosition = settingsButton.position;
        settingsButton.anchorMin = new Vector2(0, 1);
        settingsButton.anchorMax = new Vector2(0, 1);
        settingsButton.position = settingsButtonPosition;
        startingSettingsButtonPosition = settingsButton.anchoredPosition;
    }

    void Update()
    {
        if (!finishedIntro) TransitionIntro();
        else if (startedGame && !playingGame && !endingGame) TransitioningToPlaying();
        else if (!playingGame && endingGame) TransitionEnding();

        if (settingsTransition) TransitionSettings();

        currentTime += Time.deltaTime;
    }


    /********************************** Conditional Update Clarity Mathods ****************************/
    void TransitionIntro()
    {
        //Fade out the White intro letters
        float percentTransitioned = Mathf.Clamp01(-Mathf.Log10(currentTime / timeForIntroTextToFade));
        Image introImage = introScreen.GetChild(0).GetComponent<Image>();
        Image unityLogo = introScreen.GetChild(1).GetComponent<Image>();

        Color color = introImage.color;
        color.a = percentTransitioned;
        introImage.color = color;

        color = unityLogo.color;
        color.a = percentTransitioned;
        unityLogo.color = color;
        
        //Fade out the Black intro background
        percentTransitioned = Mathf.Clamp01(1 - Mathf.Pow((currentTime - timeForIntroTextToFade)/ timeForIntroToFade, 3));

        mixer.SetFloat("FootstepVolume", percentTransitioned * -80);

        introImage = introScreen.GetComponent<Image>();
        color = introImage.color;

        color.a = percentTransitioned;
        introImage.color = color;

        if (percentTransitioned <= 0.1f)
        {
            Destroy(introScreen.gameObject);
            finishedIntro = true;
        }
    }

    void TransitioningToPlaying()
    {
        float percentTransitioned = Mathf.Clamp01((currentTime - startingPlayTransitionTime) / timeForTitleToTransition);
        Camera.main.transform.position = Vector3.Lerp(startingTitlePosition, startingInGameCameraLocation.position, percentTransitioned);
        Camera.main.transform.rotation = Quaternion.Lerp(startingTitleRotation, startingInGameCameraLocation.rotation, percentTransitioned);

        percentTransitioned = Mathf.Clamp01((currentTime - startingPlayTransitionTime) / timeForTitleToFade);
        settingsButton.anchoredPosition = Vector2.Lerp(startingSettingsButtonPosition, inGameSettingsButtonPosition, percentTransitioned);

        for (int i = 0; i < titleScreen.childCount; i++)
        {
            Image current = titleScreen.GetChild(i).GetComponent<Image>();
            Color color = current.color;

            percentTransitioned = 0.5f - Mathf.Clamp01((currentTime - startingPlayTransitionTime) / timeForTitleToFade);
            color.a = percentTransitioned;
            current.color = color;
        }

        if (currentTime - startingPlayTransitionTime >= timeForTitleToTransition) PlayGame();
    }

    void TransitionEnding()
    {
        //Fade the Black ending background
        float percentTransitioned;
        Color color;

        if (endingStep == 0)
        {
            percentTransitioned = Mathf.Clamp01(1 + Mathf.Log10((currentTime - startingEndTransitionTime) / (timeForEndToFade)));

            Image partOfScreen = endScreen.GetComponent<Image>();
            color = partOfScreen.color;

            mixer.SetFloat("FootstepVolume", percentTransitioned * -80);

            color.a = percentTransitioned;
            partOfScreen.color = color;

            if (percentTransitioned == 1)
            {
                startingEndTransitionTime = currentTime;
                endingStep++;
            }
        }
        else if (endingStep == 1 || endingStep == 2)
        {
            //Fade the White ending letters
            if (endingStep == 1)
                percentTransitioned = Mathf.Clamp01(Mathf.Pow((currentTime - startingEndTransitionTime) / timeForEndTextToFade, 3));
            else
                percentTransitioned = Mathf.Clamp01(1 - Mathf.Pow((currentTime - startingEndTransitionTime) / timeForEndTextToFade, 3));
            
            Text textOfScreen = endScreen.GetChild(0).GetComponent<Text>();
            color = textOfScreen.color;

            color.a = percentTransitioned;
            textOfScreen.color = color;

            if (endingStep == 1 && percentTransitioned == 1)
            {
                startingEndTransitionTime = currentTime;
                endButton.gameObject.SetActive(true);
                endingStep++;
            }
            else if (endingStep == 2 && percentTransitioned == 0)
            {
                startingEndTransitionTime = currentTime;
                endingStep++;
            }
        }
        else if(endingStep == 3)
        {
            percentTransitioned = Mathf.Clamp01(Mathf.Pow((currentTime - startingEndTransitionTime) / timeForEndCreditsToFade, 3));

            for (int i = 0; i < creditTexts.Length; i++)
            {
                color = creditTexts[i].faceColor;
                color.a = percentTransitioned;
                creditTexts[i].faceColor = color;
            }

            if (percentTransitioned == 1)
            {
                endingGame = false;
                gameEnd = true;
            }
        }
    }

    void TransitionSettings()
    {
        Vector3 startingScale = settingsIsOpen ? openSettingsScale : closedSettingsScale;
        Vector3 endingScale = settingsIsOpen ? closedSettingsScale : openSettingsScale;
        Vector2 startingPosition = settingsIsOpen ? openSettingsPosition : settingsButton.anchoredPosition;
        Vector2 endingPosition = settingsIsOpen ? settingsButton.anchoredPosition : openSettingsPosition;

        float percentTransitioned = Mathf.Clamp01((currentTime - startingSettingsTransitionTime) / timeForSettingsToOpen);

        Vector3 newScale = Vector3.Lerp(startingScale, endingScale, percentTransitioned);
        Vector2 newPosition = Vector2.Lerp(startingPosition, endingPosition, percentTransitioned);

        settingsScreen.anchoredPosition = newPosition;
        settingsScreen.localScale = newScale;

        if (percentTransitioned == 1)
        {
            settingsTransition = false;
            settingsIsOpen = !settingsIsOpen;
        }
    }


    /********************************* Public Methods ****************************/
    public void OnPlay()
    {
        if (!startedGame && !playingGame && !gameEnd)
        {
            startedGame = true;
            startingPlayTransitionTime = currentTime;
            source.Play();
            audioManager.StartTransition(1);

            if (settingsIsOpen) OnSettings();
        }
    }

    public void OnExit() => Application.Quit();

    public void OnSettings()
    {
        if (!settingsTransition && (finishedIntro && !startedGame || playingGame && !endingGame))
        {
            settingsTransition = true;
            startingSettingsTransitionTime = currentTime;
            source.Play();
        }
    }

    public void OnEnd()
    {
        if (!endingGame && !gameEnd)
        {
            endingGame = true;
            playingGame = false;
            startingEndTransitionTime = currentTime;
            audioManager.StartTransition(2);
            endScreen.gameObject.SetActive(true);

            if (settingsIsOpen) OnSettings();
        }
    }

    public void DecreaseAdeptsLeftCount() => --AdeptsCount;


    /****************** Helper Methods *********************************/
    void PlayGame()
    {
        if (!gameEnd)
        {
            Camera.main.GetComponent<CameraController>().enabled = true;
            titleScreen.gameObject.SetActive(false);
            playingGame = true;
            player.GameStart();
            introCharacterCollider.enabled = true;
            mixer.SetFloat("FootstepVolume", 0);
        }
    }
}
