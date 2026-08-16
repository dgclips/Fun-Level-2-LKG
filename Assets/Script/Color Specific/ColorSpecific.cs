using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColorSpecific : MonoBehaviour
{
   [SerializeField] private List<ColorChange> images;
   [SerializeField] private List<ColorButton> colors;

   [SerializeField] private Color lightBlue;
   [SerializeField] private bool IsNotAlpha;

   [Header("Animation")]
   [SerializeField] private float selectPunchScale = 0.25f;
   [SerializeField] private float correctPopDuration = 0.3f;
   [SerializeField] private float wrongShakeDuration = 0.35f;
   [SerializeField] private float wrongShakeStrength = 20f;

   private Color _currentColor = Color.yellow;

   [System.Serializable]
   public class ColorChange
   {
      public Button button;          // Image is obtained automatically
      public Color correctColor;     // Assign the correct color for this image
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

            // Already colored
            if (img.color != Color.white)
               return;

            // Apply only if selected color matches the correct color
            if (IsSameColor(_currentColor, item.correctColor))
            {
               img.color = item.correctColor;
               PlayCorrectPop(img.transform);
               CheckWhite();
            }
            else
            {
               EventManager.WrongAnswer();
               AudioManager.audioManager.Play("wrong");
               PlayWrongShake(img.transform);
            }
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

   private bool IsSameColor(Color a, Color b)
   {
      const float tolerance = 0.01f;

      return Mathf.Abs(a.r - b.r) < tolerance &&
             Mathf.Abs(a.g - b.g) < tolerance &&
             Mathf.Abs(a.b - b.b) < tolerance;
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
   /// Satisfying bounce-in used when an image is colored correctly.
   /// </summary>
   private void PlayCorrectPop(Transform image)
   {
      image.DOKill();
      image.localScale = Vector3.one * 0.85f;

      image.DOScale(1f, correctPopDuration)
           .SetEase(Ease.OutBack)
           .SetUpdate(true);
   }

   /// <summary>
   /// Shake used to flag a wrong color choice.
   /// </summary>
   private void PlayWrongShake(Transform image)
   {
      image.DOKill();
      image.localScale = Vector3.one;

      image.DOShakePosition(wrongShakeDuration, wrongShakeStrength, 12, 90, false, true)
           .SetUpdate(true);
   }
}