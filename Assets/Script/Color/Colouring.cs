using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Colouring : MonoBehaviour
{
   [SerializeField] private List<ColorChange> images;
   [SerializeField] private List<ColorButton> colors;

   [SerializeField] private Color lightBlue;
   [SerializeField] private bool IsNotAlpha;

   private Color _currentColor = Color.yellow;

   [System.Serializable]
   public class ColorChange
   {
      public Button button;   // Assign only the Button (Image is obtained automatically)
   }

   [System.Serializable]
   public class ColorButton
   {
      public Color color;
      public Button button;
   }

   private void OnEnable()
   {
      Reset();
   }

   private void Start()
   {
      // Setup colouring buttons
      foreach (var item in images)
      {
         Image img = item.button.image;

         if (!IsNotAlpha)
            img.alphaHitTestMinimumThreshold = 0.1f;

         item.button.onClick.AddListener(() =>
         {
            AudioManager.audioManager.Play("click");
            img.color = _currentColor;
            CheckWhite();
         });
      }

      // Setup color palette buttons
      foreach (var color in colors)
      {
         Color selectedColor = color.color;

         color.button.onClick.AddListener(() =>
         {
            AudioManager.audioManager.Play("click");
            _currentColor = selectedColor;
         });
      }
   }

   private void CheckWhite()
   {
      int coloredCount = 0;

      foreach (var item in images)
      {
         if (item.button.image.color != Color.white)
            coloredCount++;
      }

      if (coloredCount == images.Count)
      {
         EventManager.GameComplete();
      }
   }

   public void Colors(Color color)
   {
      AudioManager.audioManager.Play("click");
      _currentColor = color;
   }

   public void RedColor()
   {
      AudioManager.audioManager.Play("click");
      _currentColor = Color.red;
   }

   public void GreenColor()
   {
      AudioManager.audioManager.Play("click");
      _currentColor = Color.green;
   }

   public void BlueColor()
   {
      AudioManager.audioManager.Play("click");
      _currentColor = Color.blue;
   }

   public void YellowColor()
   {
      AudioManager.audioManager.Play("click");
      _currentColor = Color.yellow;
   }

   public void LightBlueColor()
   {
      AudioManager.audioManager.Play("click");
      _currentColor = lightBlue;
   }

   public void Reset()
   {
      foreach (var item in images)
      {
         item.button.image.color = Color.white;
      }
   }
}