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

      Reset();
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
         EventManager.WrongAnswer();
         AudioManager.audioManager.Play("wrong");

         PlayWrongShake(btnData.circle.transform);
      }
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
         btnData.circle.enabled = false;
         btnData.button.interactable = true;
      }

      AudioManager.audioManager.Play("button");
   }

   private void OnEnable()
   {
      count = 0;
      selectedColor = CircleColor.Red;

      foreach (ButtonData btnData in buttons)
      {
         btnData.circle.transform.DOKill();
         btnData.circle.transform.localScale = Vector3.one;
         btnData.circle.enabled = false;
         btnData.button.interactable = true;
      }
   }
}