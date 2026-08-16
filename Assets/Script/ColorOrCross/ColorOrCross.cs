using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ColorOrCross : MonoBehaviour
{
   public enum ToolType
   {
      None,
      Color,
      Cross
   }

   [Header("Questions")]
   [SerializeField] private List<Question> questions;

   [Header("Color Buttons")]
   [SerializeField] private List<ColorButton> colorButtons;

   [Header("Cross Tool Button")]
   [SerializeField] private Button crossToolButton;

   public int totalItem;

   [Header("Animation")]
   [SerializeField] private float selectPunchScale = 0.25f;
   [SerializeField] private float correctPopDuration = 0.3f;
   [SerializeField] private float wrongShakeDuration = 0.35f;
   [SerializeField] private float wrongShakeStrength = 20f;

   private int count;

   private ToolType currentTool = ToolType.None;
   private Color currentColor = Color.white;

   [System.Serializable]
   public class Question
   {
      [Header("Image")]
      public Image image;

      [Header("Cross Mark")]
      public GameObject crossMark;

      [Header("True = Cross, False = Color")]
      public bool isCross;

      [HideInInspector]
      public bool completed;
   }

   [System.Serializable]
   public class ColorButton
   {
      public Button button;
      public Color color;
   }

   private void Start()
   {
      // Register color buttons
      foreach (var c in colorButtons)
      {
         ColorButton temp = c;
         temp.button.onClick.AddListener(() => SelectColor(temp.color, temp.button.transform));
      }

      // Register cross tool
      if (crossToolButton != null)
         crossToolButton.onClick.AddListener(SelectCrossTool);

      // Register image buttons
      foreach (var q in questions)
      {
         Question temp = q;

         Button btn = temp.image.GetComponent<Button>();

         if (btn == null)
         {
            Debug.LogError(temp.image.name + " needs a Button component.");
            continue;
         }

         btn.onClick.AddListener(() => OnQuestionClicked(temp));
      }

      Reset();
   }

   //-------------------------------------------------------

   void SelectColor(Color color, Transform buttonTransform)
   {
      currentTool = ToolType.Color;
      currentColor = color;

      AudioManager.audioManager.Play("button");
      PunchSelect(buttonTransform);
   }

   //-------------------------------------------------------

   void SelectCrossTool()
   {
      currentTool = ToolType.Cross;

      AudioManager.audioManager.Play("button");
      PunchSelect(crossToolButton.transform);
   }

   //-------------------------------------------------------

   void OnQuestionClicked(Question question)
   {
      if (question.completed)
         return;

      switch (currentTool)
      {
         case ToolType.None:

            AudioManager.audioManager.Play("wrong");
            EventManager.WrongAnswer();
            PlayWrongShake(question.image.transform);
            return;

         //------------------------------------------------

         case ToolType.Color:

            if (question.isCross)
            {
               AudioManager.audioManager.Play("wrong");
               EventManager.WrongAnswer();
               PlayWrongShake(question.image.transform);
               return;
            }

            question.image.color = currentColor;
            PlayCorrectPop(question.image.transform);
            CompleteQuestion(question);
            return;

         //------------------------------------------------

         case ToolType.Cross:

            if (!question.isCross)
            {
               AudioManager.audioManager.Play("wrong");
               EventManager.WrongAnswer();
               PlayWrongShake(question.image.transform);
               return;
            }

            if (question.crossMark != null)
            {
               question.crossMark.SetActive(true);
               PlayCorrectPop(question.crossMark.transform);
            }

            CompleteQuestion(question);
            return;
      }
   }

   //-------------------------------------------------------

   /// <summary>
   /// Quick punch-scale used to highlight the just-picked tool button.
   /// </summary>
   private void PunchSelect(Transform target)
   {
      target.DOKill();
      target.localScale = Vector3.one;

      target.DOPunchScale(Vector3.one * selectPunchScale, 0.3f, 6, 0.8f)
            .SetUpdate(true);
   }

   /// <summary>
   /// Satisfying bounce-in used when a question is answered correctly.
   /// </summary>
   private void PlayCorrectPop(Transform target)
   {
      target.DOKill();
      target.localScale = Vector3.one * 0.6f;

      target.DOScale(1f, correctPopDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
   }

   /// <summary>
   /// Shake used to flag a wrong tool/answer choice.
   /// </summary>
   private void PlayWrongShake(Transform target)
   {
      target.DOKill();
      target.localScale = Vector3.one;

      target.DOShakePosition(wrongShakeDuration, wrongShakeStrength, 12, 90, false, true)
            .SetUpdate(true);
   }

   //-------------------------------------------------------

   void CompleteQuestion(Question question)
   {
      question.completed = true;

      count++;

      AudioManager.audioManager.Play("correct");

      if (count >= totalItem)
      {
         EventManager.GameComplete();
      }
   }

   //-------------------------------------------------------

   public void Reset()
   {
      count = 0;

      currentTool = ToolType.None;

      foreach (var q in questions)
      {
         q.completed = false;

         q.image.transform.DOKill();
         q.image.transform.localScale = Vector3.one;
         q.image.color = Color.white;

         if (q.crossMark != null)
         {
            q.crossMark.transform.DOKill();
            q.crossMark.transform.localScale = Vector3.one;
            q.crossMark.SetActive(false);
         }
      }

      AudioManager.audioManager.Play("button");
   }

   //-------------------------------------------------------

   private void OnEnable()
   {
      count = 0;

      currentTool = ToolType.None;

      foreach (var q in questions)
      {
         q.completed = false;

         q.image.transform.DOKill();
         q.image.transform.localScale = Vector3.one;
         q.image.color = Color.white;

         if (q.crossMark != null)
         {
            q.crossMark.transform.DOKill();
            q.crossMark.transform.localScale = Vector3.one;
            q.crossMark.SetActive(false);
         }
      }
   }
}