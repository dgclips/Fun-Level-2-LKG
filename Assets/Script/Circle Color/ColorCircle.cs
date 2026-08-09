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

      // Optional:
      // Highlight selected color button here if required.
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

         if (count >= totalItem)
         {
            EventManager.GameComplete();
         }
      }
      else
      {
         EventManager.WrongAnswer();
         AudioManager.audioManager.Play("wrong");
      }
   }

   public void Reset()
   {
      count = 0;
      selectedColor = CircleColor.Red;

      foreach (ButtonData btnData in buttons)
      {
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
         btnData.circle.enabled = false;
         btnData.button.interactable = true;
      }
   }
}