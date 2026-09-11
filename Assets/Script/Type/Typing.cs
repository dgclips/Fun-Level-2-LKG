using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Typing : MonoBehaviour
{
    [SerializeField] List<CheckNumber> inputFields;



    [System.Serializable]
    public class CheckNumber
    {
        public InputField field;
        public string word;
    }
    private void OnEnable()
    {
        EventManager.OnComplete += LockInteraction;
        Reset();
        Invoke("ExitFullScreen",1f);
    }

    private void OnDisable()
    {
        EventManager.OnComplete -= LockInteraction;
    }

    /// <summary>
    /// Once the whole activity is complete, stop the player from still being
    /// able to edit the answer fields while the congrats celebration is up.
    /// Reset() is what turns interaction back on.
    /// </summary>
    private void LockInteraction()
    {
        foreach (var field in inputFields)
        {
            field.field.interactable = false;
        }
    }

    void ExitFullScreen()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        FullscreenController.instance.ExitFullscreenMode();
#endif
        Canvas.ForceUpdateCanvases();
    }
    private void Start()
    {
        foreach (var field in inputFields)
        {
            string expectedNumber = field.word;
            InputField inputField = field.field;
            inputField.onValueChanged.AddListener((string value) => CheckAnswer());
        }
    }

    void CheckAnswer()
    {
        int count = 0;
        foreach (var field in inputFields)
        {
            if (field.field.text.ToLower() == field.word.ToLower())
            {
                count++;
            }
        }

        if (count == inputFields.Count)
        {
            EventManager.GameComplete();
        }
    }

    public void Reset()
    {
      AudioManager.audioManager.Play("button");
        foreach (var field in inputFields)
        {
            field.field.text = string.Empty;
            field.field.interactable = true;
        }
    }


}
