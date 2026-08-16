using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CircleSameColor : MonoBehaviour
{
    [System.Serializable]
    public class ButtonData
    {
        public Button button;
        public Image image;
        public bool isCorrect;
    }

    public int totalItem;
    int count;
    public ButtonData[] buttons;

    [Header("Animation")]
    [SerializeField] private float correctPopDuration = 0.35f;
    [SerializeField] private float wrongShakeDuration = 0.35f;
    [SerializeField] private float wrongShakeStrength = 20f;

    void Start()
    {
        foreach (ButtonData btnData in buttons)
        {
            //btnData.image.gameObject.SetActive(false); 
            btnData.button.onClick.AddListener(() => OnButtonClick(btnData));
        }
    }

   void OnButtonClick(ButtonData btnData)
   {
      if (btnData.isCorrect)
      {
         count++;
         AudioManager.audioManager.Play("correct");
         btnData.image.enabled = true;
         btnData.button.interactable = false;

         PlayCorrectPop(btnData.image.transform);

         foreach (ButtonData but in buttons)
         {
            if (!but.isCorrect)
            {
               but.image.enabled = false;
            }
         }

         if (count == totalItem)
         {
            EventManager.GameComplete();
         }
      }
      else
      {
         btnData.image.enabled = true;

         EventManager.WrongAnswer();
         AudioManager.audioManager.Play("wrong");

         PlayWrongShake(btnData.image.transform);
      }
   }

   /// <summary>
   /// Satisfying bounce-in used when the correct circle is revealed.
   /// </summary>
   private void PlayCorrectPop(Transform image)
   {
      image.DOKill();
      image.localScale = Vector3.zero;

      image.DOScale(1f, correctPopDuration)
           .SetEase(Ease.OutBack)
           .SetUpdate(true);
   }

   /// <summary>
   /// Shake used to flag a wrong circle choice.
   /// </summary>
   private void PlayWrongShake(Transform image)
   {
      image.DOKill();
      image.localScale = Vector3.one;

      image.DOShakePosition(wrongShakeDuration, wrongShakeStrength, 12, 90, false, true)
           .SetUpdate(true);
   }

   public void Reset()
    {
        count = 0;

        foreach (ButtonData but in buttons)
        {
            but.image.transform.DOKill();
            but.image.transform.localScale = Vector3.one;
            but.image.enabled=false;
            but.button.interactable = true;
        }
      AudioManager.audioManager.Play("button");
   }
    private void OnEnable()
    {
      count = 0;

      foreach (ButtonData but in buttons)
      {
         but.image.transform.DOKill();
         but.image.transform.localScale = Vector3.one;
         but.image.enabled = false;
         but.button.interactable = true;
      }
   }
}
