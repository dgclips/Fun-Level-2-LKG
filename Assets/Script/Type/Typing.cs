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
        Reset();
        Invoke("ExitFullScreen",1f);
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
        }
    }


}
