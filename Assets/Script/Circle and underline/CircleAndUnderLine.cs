using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircleAndUnderLine : MonoBehaviour
{
   public enum SelectionType
   {
      Circle,
      Underline
   }

   [System.Serializable]
   public class CircleUnderline
   {
      public Button Item_1;
      public Button Item_2;

      public GameObject Item_1_Circle;
      public GameObject Item_2_Circle;

      public GameObject Item_1_UnderLine;
      public GameObject Item_2_UnderLine;

      // True = Circle, False = Underline
      public bool IsItem_1_Circle;
      public bool IsItem_2_Circle;

      [HideInInspector] public bool Item1Completed;
      [HideInInspector] public bool Item2Completed;
   }

   [Header("Tool Buttons")]
   [SerializeField] private Button circleButton;
   [SerializeField] private Button underlineButton;

   [SerializeField] private List<CircleUnderline> underlineList = new();

   private SelectionType currentSelection = SelectionType.Circle;

   private void Start()
   {
      circleButton.onClick.AddListener(() => currentSelection = SelectionType.Circle);
      underlineButton.onClick.AddListener(() => currentSelection = SelectionType.Underline);

      for (int i = 0; i < underlineList.Count; i++)
      {
         int index = i;

         underlineList[index].Item_1.onClick.AddListener(() => CheckAnswer(index, 1));
         underlineList[index].Item_2.onClick.AddListener(() => CheckAnswer(index, 2));
      }
   }

   private void CheckAnswer(int index, int item)
   {
      CircleUnderline question = underlineList[index];

      bool correctIsCircle = (item == 1)
          ? question.IsItem_1_Circle
          : question.IsItem_2_Circle;

      bool userSelectedCircle = currentSelection == SelectionType.Circle;

      if (correctIsCircle == userSelectedCircle)
      {
         AudioManager.audioManager.Play("correct");

         if (item == 1 && !question.Item1Completed)
         {
            question.Item1Completed = true;

            if (correctIsCircle)
               question.Item_1_Circle.SetActive(true);
            else
               question.Item_1_UnderLine.SetActive(true);
         }

         if (item == 2 && !question.Item2Completed)
         {
            question.Item2Completed = true;

            if (correctIsCircle)
               question.Item_2_Circle.SetActive(true);
            else
               question.Item_2_UnderLine.SetActive(true);
         }

         CheckActivityComplete();
      }
      else
      {
         EventManager.WrongAnswer();
         AudioManager.audioManager.Play("wrong");
      }
   }

   private void CheckActivityComplete()
   {
      foreach (var question in underlineList)
      {
         if (!question.Item1Completed || !question.Item2Completed)
            return;
      }

      EventManager.GameComplete();

      // Example:
      // successPanel.SetActive(true);
      // AudioManager.Instance.PlaySuccess();
      // Confetti.Play();
   }
   public void ResetActivity()
   {
      foreach (var question in underlineList)
      {
         // Reset completion flags
         question.Item1Completed = false;
         question.Item2Completed = false;

         // Hide visuals
         if (question.Item_1_Circle != null)
            question.Item_1_Circle.SetActive(false);

         if (question.Item_2_Circle != null)
            question.Item_2_Circle.SetActive(false);

         if (question.Item_1_UnderLine != null)
            question.Item_1_UnderLine.SetActive(false);

         if (question.Item_2_UnderLine != null)
            question.Item_2_UnderLine.SetActive(false);

         // Re-enable buttons
         question.Item_1.interactable = true;
         question.Item_2.interactable = true;
      }

      // Default selected tool
      currentSelection = SelectionType.Circle;

      AudioManager.audioManager.Play("button");
   }
}