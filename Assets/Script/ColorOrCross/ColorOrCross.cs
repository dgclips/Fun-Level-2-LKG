using System.Collections.Generic;
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
         temp.button.onClick.AddListener(() => SelectColor(temp.color));
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

   void SelectColor(Color color)
   {
      currentTool = ToolType.Color;
      currentColor = color;

      AudioManager.audioManager.Play("button");
   }

   //-------------------------------------------------------

   void SelectCrossTool()
   {
      currentTool = ToolType.Cross;

      AudioManager.audioManager.Play("button");
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
            return;

         //------------------------------------------------

         case ToolType.Color:

            if (question.isCross)
            {
               AudioManager.audioManager.Play("wrong");
               EventManager.WrongAnswer();
               return;
            }

            question.image.color = currentColor;
            CompleteQuestion(question);
            return;

         //------------------------------------------------

         case ToolType.Cross:

            if (!question.isCross)
            {
               AudioManager.audioManager.Play("wrong");
               EventManager.WrongAnswer();
               return;
            }

            if (question.crossMark != null)
               question.crossMark.SetActive(true);

            CompleteQuestion(question);
            return;
      }
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

         q.image.color = Color.white;

         if (q.crossMark != null)
            q.crossMark.SetActive(false);
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

         q.image.color = Color.white;

         if (q.crossMark != null)
            q.crossMark.SetActive(false);
      }
   }
}