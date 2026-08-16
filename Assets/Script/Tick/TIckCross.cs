using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TIckCross : MonoBehaviour
{
   [SerializeField] List<TickOrCross> questions;
   public int totalItem;
   int count;

   [System.Serializable]
   public class TickOrCross
   {
      public Button tickButton;
      public Button crossButton;
      public bool isTick;
   }

   [Header("Animation")]
   [SerializeField] private float correctPunchScale = 0.25f;
   [SerializeField] private float loserShrinkDuration = 0.2f;
   [SerializeField] private float wrongShakeDuration = 0.35f;
   [SerializeField] private float wrongShakeStrength = 20f;

   void Start()
   {
      foreach (var tick in questions)
      {
         tick.tickButton.onClick.AddListener(() => Tick(tick));
         tick.crossButton.onClick.AddListener(() => Cross(tick));
      }
   }

   public void Tick(TickOrCross tick)
   {
      if (tick.isTick)
      {
         count++;

         // Lock this question so re-clicking the same (already correct) button can't inflate the count.
         tick.tickButton.interactable = false;

         // Disable the wrong button GameObject
         AnimateAwayLoser(tick.crossButton.gameObject);
         PlayCorrectPunch(tick.tickButton.transform);

         if (count == totalItem)
            EventManager.GameComplete();

         AudioManager.audioManager.Play("correct");
      }else
      {
         EventManager.WrongAnswer();
         AudioManager.audioManager.Play("wrong");

         PlayWrongShake(tick.tickButton.transform);
      }
   }

   public void Cross(TickOrCross cross)
   {
      if (!cross.isTick)
      {
         count++;

         // Lock this question so re-clicking the same (already correct) button can't inflate the count.
         cross.crossButton.interactable = false;

         // Disable the wrong button GameObject
         AnimateAwayLoser(cross.tickButton.gameObject);
         PlayCorrectPunch(cross.crossButton.transform);

         if (count == totalItem)
            EventManager.GameComplete();

         AudioManager.audioManager.Play("correct");
      }else
      {
         EventManager.WrongAnswer();
         AudioManager.audioManager.Play("wrong");

         PlayWrongShake(cross.crossButton.transform);
      }
   }

   /// <summary>
   /// Punch feedback for the button that was correctly chosen.
   /// </summary>
   private void PlayCorrectPunch(Transform target)
   {
      target.DOKill();
      target.localScale = Vector3.one;

      target.DOPunchScale(Vector3.one * correctPunchScale, 0.3f, 6, 0.8f)
            .SetUpdate(true);
   }

   /// <summary>
   /// Shrinks the losing (unchosen) button away before deactivating it,
   /// instead of an instant SetActive(false).
   /// </summary>
   private void AnimateAwayLoser(GameObject loser)
   {
      loser.transform.DOKill();

      loser.transform.DOScale(0f, loserShrinkDuration)
           .SetEase(Ease.InBack)
           .SetUpdate(true)
           .OnComplete(() => loser.SetActive(false));
   }

   /// <summary>
   /// Shake used to flag a wrong tick/cross choice.
   /// </summary>
   private void PlayWrongShake(Transform target)
   {
      target.DOKill();
      target.localScale = Vector3.one;

      target.DOShakePosition(wrongShakeDuration, wrongShakeStrength, 12, 90, false, true)
            .SetUpdate(true);
   }

   public void Reset()
   {
      count = 0;

      foreach (var tick in questions)
      {
         // Enable both button GameObjects
         ResetButton(tick.tickButton);
         ResetButton(tick.crossButton);
      }

      AudioManager.audioManager.Play("button");
   }

   private void OnEnable()
   {
      count = 0;

      foreach (var tick in questions)
      {
         // Enable both button GameObjects
         ResetButton(tick.tickButton);
         ResetButton(tick.crossButton);
      }
   }

   /// <summary>
   /// Reactivates a button and resets its scale/tween state for the next playthrough.
   /// </summary>
   private void ResetButton(Button button)
   {
      button.transform.DOKill();
      button.transform.localScale = Vector3.one;
      button.gameObject.SetActive(true);
      button.interactable = true;
   }
}