using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueBoxController : MonoBehaviour
{
    public static int TOTAL_PROFILES = 9;
    public static int TOTAL_LORE_STORIES = 4;

#pragma warning disable CS0649 // #warning directive
    [SerializeField] float textTypingSpeed;
    [SerializeField] SceneManager sceneManager;
    [SerializeField] UnityEngine.Audio.AudioMixer mixer;
    [SerializeField] float waitingSpeed;
    [SerializeField] float inactiveYPos;
    [SerializeField] float activeYPos;
    [SerializeField] RectTransform namePlate;
    [SerializeField] Text nameElement;
    [SerializeField] TMP_Text textElement;
    [SerializeField] Image profile;
    [SerializeField] Image blackFadeScreen;
    [SerializeField] float fadeDuration;
    [SerializeField] Image storyBackground;
    [SerializeField] TMP_Text storyText;
    [SerializeField] string[] defaultNames = new string[TOTAL_PROFILES];
    [SerializeField] Sprite[] allProfiles = new Sprite[TOTAL_PROFILES];
    [SerializeField] AudioClip[] allSpeakingBlips = new AudioClip[TOTAL_PROFILES];
    [SerializeField] Vector2[] profilePositions = new Vector2[TOTAL_PROFILES];
    [SerializeField] Sprite[] allStoryBackgrounds = new Sprite[TOTAL_LORE_STORIES];
    [SerializeField] Rect[] allStoryTextPositions = new Rect[TOTAL_LORE_STORIES];

    [SerializeField] [TextArea(1, 10)] string[] Faacio = new string[10];
    [SerializeField] [TextArea(1, 10)] string[] Arscant = new string[10];
    [SerializeField] [TextArea(1, 10)] string[] Cassiotz = new string[10];
    [SerializeField] [TextArea(1, 10)] string[] Belluti = new string[10];

    public bool IsTalking { private set; get; } = false;
    public bool WaitingToContinue { set; get; } = true;
    public bool Autocomplete { set; private get; } = false;

    RectTransform rect;
    AudioSource source;
    public List<string>[] allStories = new List<string>[TOTAL_LORE_STORIES];

    void Start()
    {
        rect = GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, inactiveYPos);
        source = GetComponent<AudioSource>();

        for (int i = 0; i < allStories.Length; i++)
        {
            string[] curStory = Faacio;
            if (i == 1) curStory = Arscant;
            else if (i == 2) curStory = Cassiotz;
            else if (i == 3) curStory = Belluti;

            allStories[i] = new List<string>();
            allStories[i].AddRange(curStory);
        }
    }


    /***************************** Original Non-System Methods Begin *************************************************************/
    public void OpenDialogueBox(string name, int characterIndex, string[] text)
        => StartCoroutine(TypeText(name, characterIndex, text));

    public void CloseDialogueBox() => StartCoroutine(CloseBox());

    IEnumerator TypeText(string name, int characterIndex, string[] text)
    {
        IsTalking = true;

        textElement.maxVisibleCharacters = 0;
        string currentPage = text[0];
        char[] totalText = text[0].ToCharArray();
        int currentIndex = characterIndex;

        if (characterIndex >= allProfiles.Length) characterIndex = 0;
        
        if (totalText[0] == '[')
        {
            int endingBracket = currentPage.IndexOf(']');
            currentIndex = totalText[endingBracket - 1] - '0';
            string currentName = endingBracket > 2 ? currentPage.Substring(1, endingBracket - 2) : "";

            ChangeBox(currentName, currentIndex);
        }
        else ChangeBox(name, characterIndex);

        yield return OpenBox();

        for (int pageNum = 0; pageNum < text.Length; pageNum++)
        {
            currentPage = text[pageNum];
            totalText = currentPage.ToCharArray();
            currentIndex = characterIndex;

            if (currentPage.StartsWith("*"))
            {
                yield return BeginLoreStory(totalText[1] - '0');
                continue;
            }
            else if (currentPage.StartsWith("["))
            {
                int endingBracket = currentPage.IndexOf(']');
                currentIndex = totalText[endingBracket - 1] - '0';
                string currentName = endingBracket > 2 ? currentPage.Substring(1, endingBracket - 2) : "";

                if (pageNum != 0) ChangeBox(currentName, currentIndex);

                currentPage = text[pageNum].Substring(endingBracket + 2);
                totalText = currentPage.ToCharArray();
            }
            else if (nameElement.text != name) ChangeBox(name, characterIndex);

            Autocomplete = false;

            yield return PrintPageOfText(currentIndex, currentPage, totalText, textElement);

            if (pageNum < text.Length - 1) yield return MoreToCome(textElement);
        }

        IsTalking = false;
        yield break;
    }

    IEnumerator PrintPageOfText(int speakingIndex, string fullPage, char[] letterArray, TMP_Text targetText)
    {
        targetText.text = fullPage + "...";
        targetText.maxVisibleCharacters = 0;
        
        for (int i = 0; i < fullPage.Length; i++)
        {
            targetText.maxVisibleCharacters = i + 1;

            if (Autocomplete)
            {
                targetText.maxVisibleCharacters = fullPage.Length;
                break;
            }

            if (letterArray[i] != ' ') source.PlayOneShot(allSpeakingBlips[speakingIndex]);

            yield return new WaitForSeconds(textTypingSpeed);
        }
        
        Autocomplete = false;

        yield break;
    }

    IEnumerator MoreToCome(TMP_Text targetText)
    {
        int baseVisible = targetText.maxVisibleCharacters;
        WaitingToContinue = false;

        for (int i = 0; !WaitingToContinue; i++)
        {
            targetText.maxVisibleCharacters = baseVisible + i;

            if (i == 3) i = 0;

            yield return new WaitForSeconds(waitingSpeed);
        }

        targetText.maxVisibleCharacters = baseVisible;
        yield break;
    }


    /********************************* Lore Story Methods ******************************************/
    IEnumerator BeginLoreStory(int storyIndex)
    {
        storyText.maxVisibleCharacters = 0;
        Autocomplete = false;

        if (allStoryBackgrounds[storyIndex] != null)
        {
            storyBackground.sprite = allStoryBackgrounds[storyIndex];
            storyText.rectTransform.anchoredPosition = allStoryTextPositions[storyIndex].position;
            storyText.rectTransform.sizeDelta = allStoryTextPositions[storyIndex].size;
        }
        
        //fade out the background for transition along with music
        float previousMusicVolume = mixer.GetFloat("MusicVolume", out float tempFloat) ? tempFloat : 0;

        Quaternion cameraRotation = Camera.main.transform.rotation;

        blackFadeScreen.gameObject.SetActive(true);
        yield return FadeBlack(true, true, Mathf.Min(-20, previousMusicVolume * 1.75f));

        storyBackground.gameObject.SetActive(true);
        textElement.maxVisibleCharacters = 0;

        Camera.main.GetComponent<CameraController>().CameraControlPaused = true;
        Camera.main.transform.rotation = Quaternion.LookRotation(Vector3.up, Vector3.back);
        Camera.main.transform.position += Camera.main.transform.forward * 20;

        for (int i = 0; i < transform.childCount; i++) 
            transform.GetChild(i).gameObject.SetActive(false);

        yield return FadeBlack(false, false, 0);

        List<string> curStory = allStories[storyIndex];

        for (int pageNum = 0; pageNum < curStory.Count; pageNum++)
        {
            string currentPage = curStory[pageNum];

            yield return PrintPageOfText(storyIndex + 5, currentPage, currentPage.ToCharArray(), storyText);

            if (pageNum < curStory.Count - 1) yield return MoreToCome(storyText);
        }

        yield return MoreToCome(textElement);
        
        yield return FadeBlack(true, false, 0);
        //While it's faded out...

        storyBackground.gameObject.SetActive(false);

        Camera.main.transform.position -= Camera.main.transform.forward * 20;
        Camera.main.transform.rotation = cameraRotation;
        Camera.main.GetComponent<CameraController>().CameraControlPaused = false;

        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(true);

        yield return FadeBlack(false, true, previousMusicVolume);
        blackFadeScreen.gameObject.SetActive(false);
        //Back to visible

        sceneManager.DecreaseAdeptsLeftCount();
        yield break;
    }

    IEnumerator FadeBlack(bool fadingIn, bool musicIsChanging, float targetVolume)
    {
        Func<float, float> FadeFormula = FadeInFormula;
        if (!fadingIn) FadeFormula = FadeOutFormula;

        float fadePercentage = fadingIn ? 0 : 1;
        float startingTime = Time.time;
        float startingVolume = mixer.GetFloat("MusicVolume", out float tempFloat) ? tempFloat : 0; ;

        while (fadingIn ? fadePercentage < 1 : fadePercentage > 0)
        {
            fadePercentage = Mathf.Clamp01((Time.time - startingTime) / fadeDuration); //gives percentage over time.

            if (musicIsChanging) mixer.SetFloat("MusicVolume", Mathf.Lerp(startingVolume, targetVolume, fadePercentage));

            fadePercentage = FadeFormula((Time.time - startingTime) / fadeDuration); //gives modified percentage.
            
            Image blackScreen = blackFadeScreen.GetComponent<Image>();
            Color color = blackScreen.color;

            color.a = fadePercentage;
            blackScreen.color = color;

            yield return new WaitForFixedUpdate();
        }

        yield break;
    }

    float FadeInFormula(float percentageFaded)
        => Mathf.Clamp01(1 + Mathf.Log10(percentageFaded));

    float FadeOutFormula(float percentageFaded)
        => Mathf.Clamp01(1 - Mathf.Pow(percentageFaded, 3));

    /****************** Basic Manipulation of the Dialogue Box Methods *****************************/
    IEnumerator OpenBox()
    {
        float percent = 0;
        float currentYPos;

        while (rect.anchoredPosition.y != activeYPos)
        {
            currentYPos = Mathf.Lerp(inactiveYPos, activeYPos, percent);
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, currentYPos);
            
            percent += 0.2f;

            yield return new WaitForSeconds(0.00001f);
        }

        yield break;
    }

    void ChangeBox(string name, int characterIndex)
    {
        float namePlateSize = 0;
        nameElement.text = name != "" ? name : defaultNames[characterIndex];
        
        namePlateSize += nameElement.text.Length * (22 - nameElement.text.Length / 2);

        namePlate.sizeDelta = new Vector2(namePlateSize, namePlate.sizeDelta.y);

        profile.sprite = allProfiles[characterIndex];
        profile.rectTransform.anchoredPosition = profilePositions[characterIndex];
    }

    IEnumerator CloseBox()
    {
        textElement.text = "";

        float percent = 0;
        float currentYPos;

        while (rect.anchoredPosition.y != inactiveYPos)
        {
            currentYPos = Mathf.Lerp(activeYPos, inactiveYPos, percent);
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, currentYPos);

            percent += 0.2f;

            yield return new WaitForSeconds(0.00001f);
        }

        yield break;
    }
}
