using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ColorCircle : MonoBehaviour
{
   public enum CircleColor
   {
      Red,
      Blue
   }

   [System.Serializable]
   public class ButtonData
   {
      public Button button;
      public Image circle;
      public CircleColor correctColor;
   }

   [Header("Items")]
   public ButtonData[] buttons;
   public int totalItem;

   [Header("Color Selection Buttons")]
   public Button redButton;
   public Button blueButton;

   [Header("Circle Colors")]
   public Color redColor = Color.red;
   public Color blueColor = Color.blue;

   [Header("Animation")]
   [SerializeField] private float selectPunchScale = 0.25f;
   [SerializeField] private float correctPopDuration = 0.35f;
   [SerializeField] private float wrongShakeDuration = 0.35f;
   [SerializeField] private float wrongShakeStrength = 20f;

   [Header("Wrong Feedback")]
   [Tooltip("Off by default to keep the other activities using this script unchanged (a wrong pick stays colored until Reset). Enable per-activity to fade a wrong pick back out on its own, so the page doesn't stay marked and the child can retry that item.")]
   [SerializeField] private bool autoFadeWrong = false;
   [Tooltip("How long the wrong color stays fully visible before it starts fading.")]
   [SerializeField] private float wrongVisibleDuration = 0.6f;
   [Tooltip("How long the fade-out itself takes once it starts.")]
   [SerializeField] private float wrongFadeDuration = 0.4f;

   private int count;
   private CircleColor selectedColor = CircleColor.Red;

   private void Start()
   {
      // Color Selection
      redButton.onClick.AddListener(() => SelectColor(CircleColor.Red));
      blueButton.onClick.AddListener(() => SelectColor(CircleColor.Blue));

      // Item Buttons
      foreach (ButtonData btnData in buttons)
      {
         ButtonData temp = btnData; // Prevent closure issue
         temp.button.onClick.AddListener(() => OnButtonClick(temp));
      }

      // OnEnable() (which always runs before Start()) already calls Reset()
      // once - calling it again here would just double up the reset sound
      // on first load.
   }

   void SelectColor(CircleColor color)
   {
      selectedColor = color;
      AudioManager.audioManager.Play("button");

      // Punch feedback so it's clear which color is now active.
      Button selectedButton = color == CircleColor.Red ? redButton : blueButton;
      PunchSelect(selectedButton.transform);
   }

   void OnButtonClick(ButtonData btnData)
   {
      // Ignore if already completed
      if (!btnData.button.interactable)
         return;

      // A previous wrong pick on this circle may still be fading out - kill that
      // tween first, otherwise it would carry on fading (and then disable) the
      // circle we are about to show. Re-assigning color also restores the alpha
      // a part-finished fade left behind.
      btnData.circle.DOKill();

      // Apply selected color to the circle
      btnData.circle.color = (selectedColor == CircleColor.Red) ? redColor : blueColor;
      btnData.circle.enabled = true;

      if (selectedColor == btnData.correctColor)
      {
         count++;

         AudioManager.audioManager.Play("correct");
         btnData.button.interactable = false;

         PlayCorrectPop(btnData.circle.transform);

         if (count >= totalItem)
         {
            EventManager.GameComplete();
         }
      }
      else
      {
         if (autoFadeWrong)
         {
            FadeOutWrong(btnData.circle);
         }

         EventManager.WrongAnswer();
         AudioManager.audioManager.Play("wrong");

         PlayWrongShake(btnData.circle.transform);
      }
   }

   /// <summary>
   /// Fades a wrongly colored circle back out on its own after a short pause,
   /// so a mistake doesn't stay marked on the page and the child can pick a
   /// different color for that same item. Only runs when autoFadeWrong is
   /// enabled on this activity. The shake runs on the transform and this fade
   /// on the Image, so the two tweens don't kill each other.
   /// </summary>
   private void FadeOutWrong(Image circle)
   {
      circle.DOFade(0f, wrongFadeDuration)
            .SetDelay(wrongVisibleDuration)
            .SetUpdate(true)
            .OnComplete(() => circle.enabled = false);
   }

   /// <summary>
   /// Quick punch-scale used to highlight the just-picked color button.
   /// </summary>
   private void PunchSelect(Transform target)
   {
      target.DOKill();
      target.localScale = Vector3.one;

      target.DOPunchScale(Vector3.one * selectPunchScale, 0.3f, 6, 0.8f)
            .SetUpdate(true);
   }

   /// <summary>
   /// Satisfying bounce-in used when a circle is colored correctly.
   /// </summary>
   private void PlayCorrectPop(Transform circle)
   {
      circle.DOKill();
      circle.localScale = Vector3.one * 0.6f;

      circle.DOScale(1f, correctPopDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
   }

   /// <summary>
   /// Shake used to flag a wrong color choice.
   /// </summary>
   private void PlayWrongShake(Transform circle)
   {
      circle.DOKill();
      circle.localScale = Vector3.one;

      circle.DOShakePosition(wrongShakeDuration, wrongShakeStrength, 12, 90, false, true)
            .SetUpdate(true);
   }

   public void Reset()
   {
      count = 0;
      selectedColor = CircleColor.Red;

      foreach (ButtonData btnData in buttons)
      {
         btnData.circle.transform.DOKill();
         btnData.circle.transform.localScale = Vector3.one;

         // Kill any in-flight wrong-answer fade and put the alpha back, so a
         // reset mid-fade can't leave a circle stuck transparent, or let that
         // tween's OnComplete disable a circle that has since been re-colored.
         btnData.circle.DOKill();
         Color c = btnData.circle.color;
         c.a = 1f;
         btnData.circle.color = c;

         btnData.circle.enabled = false;
         btnData.button.interactable = true;
      }

      redButton.interactable = true;
      blueButton.interactable = true;

      AudioManager.audioManager.Play("button");
   }

   private void OnEnable()
   {
      EventManager.OnComplete += LockInteraction;
      Reset();
   }

   private void OnDisable()
   {
      EventManager.OnComplete -= LockInteraction;
   }

   /// <summary>
   /// Once the whole activity is complete, stop the player from still being
   /// able to click the color-selection buttons while the congrats
   /// celebration is up. Reset() is what turns interaction back on.
   /// </summary>
   private void LockInteraction()
   {
      foreach (ButtonData btnData in buttons)
      {
         btnData.button.interactable = false;
      }

      redButton.interactable = false;
      blueButton.interactable = false;
   }
}