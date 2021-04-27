using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreBasedAdjustment : MonoBehaviour
{
    [SerializeField] SceneManager sceneManager;
    [SerializeField] string[] speakingTextReplacements;
    [SerializeField] int[] atWhatAdeptCountToReplaceText;
    [SerializeField] int[] speakingTextReplacementIndecies;
    //the x corrosponds to the speakingTextReplacement index,
    //and the y corrosponds to the speakingObject's text index to be replaced.

    TalkingObject talkingObject;
    int internalAdepts;

    void Start()
    {
        talkingObject = GetComponent<TalkingObject>();
        internalAdepts = sceneManager.AdeptsCount;
    }

    void Update()
    {
        if (internalAdepts != sceneManager.AdeptsCount)
        {
            for (int i = 0; i < atWhatAdeptCountToReplaceText.Length; i++)
            {
                if (atWhatAdeptCountToReplaceText[i] == sceneManager.AdeptsCount)
                {
                    int curIndex = speakingTextReplacementIndecies[i];
                    if (curIndex > talkingObject.speakingText.Length)
                    {
                        string[] temp = new string[curIndex + 1]; 
                        
                        for (int j = 0; j < talkingObject.speakingText.Length; j++)
                            temp[j] = talkingObject.speakingText[j];

                        talkingObject.speakingText = temp;
                    }

                    talkingObject.speakingText[curIndex] = speakingTextReplacements[i];
                }
            }

            internalAdepts = sceneManager.AdeptsCount;
        }
    }

    void OnValidate()
    {
        if (atWhatAdeptCountToReplaceText.Length != speakingTextReplacements.Length)
        {
            int[] temp = new int[speakingTextReplacements.Length];
            int minLength = Mathf.Min(speakingTextReplacements.Length, atWhatAdeptCountToReplaceText.Length);

            for (int i = 0; i < minLength; i++)
                temp[i] = atWhatAdeptCountToReplaceText[i];

            atWhatAdeptCountToReplaceText = temp;
        }

        if (speakingTextReplacementIndecies.Length != speakingTextReplacements.Length)
        {
            int[] temp = new int[speakingTextReplacements.Length];
            int minLength = Mathf.Min(speakingTextReplacements.Length, speakingTextReplacementIndecies.Length);

            for (int i = 0; i < minLength; i++)
                temp[i] = speakingTextReplacementIndecies[i];
            speakingTextReplacementIndecies = temp;
        }
    }
}
