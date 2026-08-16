using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Colouring : MonoBehaviour
{
   [SerializeField] private List<ColorChange> images;
   [SerializeField] private List<ColorButton> colors;

   [SerializeField] private Color lightBlue;
   [SerializeField] private bool IsNotAlpha;

   [Header("Animation")]
   [SerializeField] private float selectPunchScale = 0.25f;
   [SerializeField] private float colorPopDuration = 0.3f;

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
            PlayColorPop(img.transform);
            CheckWhite();
         });
      }

      // Setup color palette buttons
      foreach (var color in colors)
      {
         Color selectedColor = color.color;
         Transform buttonTransform = color.button.transform;

         color.button.onClick.AddListener(() =>
         {
            AudioManager.audioManager.Play("click");
            _currentColor = selectedColor;
            PunchSelect(buttonTransform);
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
      PunchClickedButton();
   }

   public void RedColor()
   {
      AudioManager.audioManager.Play("click");
      _currentColor = Color.red;
      PunchClickedButton();
   }

   public void GreenColor()
   {
      AudioManager.audioManager.Play("click");
      _currentColor = Color.green;
      PunchClickedButton();
   }

   public void BlueColor()
   {
      AudioManager.audioManager.Play("click");
      _currentColor = Color.blue;
      PunchClickedButton();
   }

   public void YellowColor()
   {
      AudioManager.audioManager.Play("click");
      _currentColor = Color.yellow;
      PunchClickedButton();
   }

   public void LightBlueColor()
   {
      AudioManager.audioManager.Play("click");
      _currentColor = lightBlue;
      PunchClickedButton();
   }

   public void Reset()
   {
      AudioManager.audioManager.Play("button");
      foreach (var item in images)
      {
         item.button.image.transform.DOKill();
         item.button.image.transform.localScale = Vector3.one;
         item.button.image.color = Color.white;
      }
   }

   /// <summary>
   /// Punches whichever UI button invoked this click - used by the public
   /// Color-selection methods (wired directly via Inspector onClick events),
   /// which don't receive a Button reference of their own.
   /// </summary>
   private void PunchClickedButton()
   {
      GameObject clicked = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;

      if (clicked != null)
         PunchSelect(clicked.transform);
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
   /// Satisfying little bounce whenever a section is freshly colored.
   /// </summary>
   private void PlayColorPop(Transform image)
   {
      image.DOKill();
      image.localScale = Vector3.one * 0.9f;

      image.DOScale(1f, colorPopDuration)
           .SetEase(Ease.OutBack)
           .SetUpdate(true);
   }
}