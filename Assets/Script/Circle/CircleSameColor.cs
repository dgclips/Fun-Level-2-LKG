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

    [Header("Wrong Feedback")]
    [Tooltip("Off by default to keep every other activity using this script unchanged (wrong picks just stay shown, whatever color is authored on them, until Reset). Enable per-activity to auto-fade a wrong pick out after a delay, always in wrongColor regardless of what's authored on its Image.")]
    [SerializeField] private bool autoFadeWrong = false;
    [SerializeField] private Color wrongColor = new Color(1f, 0.15f, 0.15f, 1f);
    [Tooltip("How long the red highlight stays fully visible before it starts fading.")]
    [SerializeField] private float wrongVisibleDuration = 0.6f;
    [Tooltip("How long the fade-out itself takes once it starts.")]
    [SerializeField] private float wrongFadeDuration = 0.4f;

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
         if (autoFadeWrong)
         {
            ShowWrongFeedback(btnData.image);
         }
         else
         {
            btnData.image.enabled = true;
         }

         EventManager.WrongAnswer();
         AudioManager.audioManager.Play("wrong");

         PlayWrongShake(btnData.image.transform);
      }
   }

   /// <summary>
   /// Always flashes a wrong pick in the same red (never whatever color
   /// happens to be authored on that Image), then fades it out on its own
   /// after a short delay - so a wrong answer doesn't stay marked forever
   /// and the player can try again without a lingering highlight. Only runs
   /// when autoFadeWrong is enabled on this activity.
   /// </summary>
   private void ShowWrongFeedback(Image image)
   {
      image.DOKill();

      Color c = wrongColor;
      c.a = 1f;
      image.color = c;
      image.enabled = true;

      image.DOFade(0f, wrongFadeDuration)
           .SetDelay(wrongVisibleDuration)
           .SetUpdate(true)
           .OnComplete(() => image.enabled = false);
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
            ResetButtonVisual(but);
        }
      AudioManager.audioManager.Play("button");
   }
    private void OnEnable()
    {
      EventManager.OnComplete += LockInteraction;

      count = 0;

      foreach (ButtonData but in buttons)
      {
         ResetButtonVisual(but);
      }
   }

   private void OnDisable()
   {
      EventManager.OnComplete -= LockInteraction;
   }

   /// <summary>
   /// Once the whole activity is complete, stop the player from still being
   /// able to click the wrong-answer buttons (the only ones that never lock
   /// themselves individually) while the congrats celebration is up. Reset()
   /// is what turns interaction back on.
   /// </summary>
   private void LockInteraction()
   {
      foreach (ButtonData but in buttons)
      {
         but.button.interactable = false;
      }
   }

   /// <summary>
   /// Stops any in-flight pop/shake/fade on a button's image and puts it back
   /// to its resting state - used by both Reset() and OnEnable() so a wrong
   /// answer's fade-out tween can never be left half-finished (faded alpha,
   /// still enabled) across a reset/replay.
   /// </summary>
   private void ResetButtonVisual(ButtonData but)
   {
      but.image.transform.DOKill();
      but.image.transform.localScale = Vector3.one;

      but.image.DOKill();
      Color c = but.image.color;
      c.a = 1f;
      but.image.color = c;

      but.image.enabled = false;
      but.button.interactable = true;
   }
}
