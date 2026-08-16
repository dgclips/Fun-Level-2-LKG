using System.Collections.Generic;
using DG.Tweening;
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

   [Header("Animation")]
   [SerializeField] private float selectPunchScale = 0.25f;
   [SerializeField] private float correctPopDuration = 0.3f;
   [SerializeField] private float wrongShakeDuration = 0.35f;
   [SerializeField] private float wrongShakeStrength = 20f;

   private SelectionType currentSelection = SelectionType.Circle;

   private void Start()
   {
      circleButton.onClick.AddListener(() =>
      {
         currentSelection = SelectionType.Circle;
         AudioManager.audioManager.Play("button");
         PunchSelect(circleButton.transform);
      });

      underlineButton.onClick.AddListener(() =>
      {
         currentSelection = SelectionType.Underline;
         AudioManager.audioManager.Play("button");
         PunchSelect(underlineButton.transform);
      });

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

            GameObject mark = correctIsCircle ? question.Item_1_Circle : question.Item_1_UnderLine;
            ShowMark(mark);
         }

         if (item == 2 && !question.Item2Completed)
         {
            question.Item2Completed = true;

            GameObject mark = correctIsCircle ? question.Item_2_Circle : question.Item_2_UnderLine;
            ShowMark(mark);
         }

         CheckActivityComplete();
      }
      else
      {
         EventManager.WrongAnswer();
         AudioManager.audioManager.Play("wrong");

         Button wrongButton = item == 1 ? question.Item_1 : question.Item_2;
         PlayWrongShake(wrongButton.transform);
      }
   }

   /// <summary>
   /// Activates the circle/underline mark with a bounce-in pop instead of
   /// snapping straight to full scale.
   /// </summary>
   private void ShowMark(GameObject mark)
   {
      if (mark == null)
         return;

      mark.SetActive(true);

      mark.transform.DOKill();
      mark.transform.localScale = Vector3.zero;

      mark.transform.DOScale(1f, correctPopDuration)
          .SetEase(Ease.OutBack)
          .SetUpdate(true);
   }

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
   /// Shake used to flag a wrong circle/underline choice.
   /// </summary>
   private void PlayWrongShake(Transform target)
   {
      target.DOKill();
      target.localScale = Vector3.one;

      target.DOShakePosition(wrongShakeDuration, wrongShakeStrength, 12, 90, false, true)
            .SetUpdate(true);
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
         ResetMark(question.Item_1_Circle);
         ResetMark(question.Item_2_Circle);
         ResetMark(question.Item_1_UnderLine);
         ResetMark(question.Item_2_UnderLine);

         // Re-enable buttons
         question.Item_1.transform.DOKill();
         question.Item_1.transform.localScale = Vector3.one;
         question.Item_1.interactable = true;

         question.Item_2.transform.DOKill();
         question.Item_2.transform.localScale = Vector3.one;
         question.Item_2.interactable = true;
      }

      // Default selected tool
      currentSelection = SelectionType.Circle;

      AudioManager.audioManager.Play("button");
   }

   private void OnEnable()
   {
      foreach (var question in underlineList)
      {
         // Reset completion flags
         question.Item1Completed = false;
         question.Item2Completed = false;

         // Hide visuals
         ResetMark(question.Item_1_Circle);
         ResetMark(question.Item_2_Circle);
         ResetMark(question.Item_1_UnderLine);
         ResetMark(question.Item_2_UnderLine);

         // Re-enable buttons
         question.Item_1.transform.DOKill();
         question.Item_1.transform.localScale = Vector3.one;
         question.Item_1.interactable = true;

         question.Item_2.transform.DOKill();
         question.Item_2.transform.localScale = Vector3.one;
         question.Item_2.interactable = true;
      }

      // Default selected tool
      currentSelection = SelectionType.Circle;
   }

   /// <summary>
   /// Deactivates a mark and resets its scale/tween state for the next playthrough.
   /// </summary>
   private void ResetMark(GameObject mark)
   {
      if (mark == null)
         return;

      mark.transform.DOKill();
      mark.transform.localScale = Vector3.one;
      mark.SetActive(false);
   }
}