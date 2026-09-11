using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SeedActivity : MonoBehaviour
{
   public enum ToolType
   {
      None,
      Cross,
      OneSeed,
      ManySeeds
   }

   [Header("Questions")]
   [SerializeField] private List<Question> questions;

   [Header("Tool Buttons")]
   [SerializeField] private Button crossButton;
   [SerializeField] private Button oneSeedButton;
   [SerializeField] private Button manySeedButton;

   public int totalItem;

   private int count;
   private ToolType currentTool = ToolType.None;

   [System.Serializable]
   public class Question
   {
      public Button imageButton;

      public GameObject crossObject;
      public GameObject oneSeedObject;
      public GameObject manySeedObject;

      public ToolType correctTool;

      [HideInInspector]
      public bool completed;
   }

   private void Start()
   {
      crossButton.onClick.AddListener(() => SelectTool(ToolType.Cross));
      oneSeedButton.onClick.AddListener(() => SelectTool(ToolType.OneSeed));
      manySeedButton.onClick.AddListener(() => SelectTool(ToolType.ManySeeds));

      foreach (var q in questions)
      {
         Question temp = q;
         temp.imageButton.onClick.AddListener(() => ClickQuestion(temp));
      }
   }

   void SelectTool(ToolType tool)
   {
      currentTool = tool;
      AudioManager.audioManager.Play("button");
   }

   void ClickQuestion(Question question)
   {
      if (question.completed)
         return;

      if (currentTool == ToolType.None)
      {
         EventManager.WrongAnswer();
         AudioManager.audioManager.Play("wrong");
         return;
      }

      if (currentTool == question.correctTool)
      {
         switch (currentTool)
         {
            case ToolType.Cross:

               if (question.crossObject != null)
                  question.crossObject.SetActive(true);

               break;

            case ToolType.OneSeed:

               if (question.oneSeedObject != null)
                  question.oneSeedObject.SetActive(true);

               break;

            case ToolType.ManySeeds:

               if (question.manySeedObject != null)
                  question.manySeedObject.SetActive(true);

               break;
         }

         question.completed = true;
         count++;

         AudioManager.audioManager.Play("correct");

         if (count == totalItem)
            EventManager.GameComplete();
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
      currentTool = ToolType.None;

      foreach (var q in questions)
      {
         q.completed = false;
         q.imageButton.interactable = true;

         if (q.crossObject != null)
            q.crossObject.SetActive(false);

         if (q.oneSeedObject != null)
            q.oneSeedObject.SetActive(false);

         if (q.manySeedObject != null)
            q.manySeedObject.SetActive(false);
      }

      crossButton.interactable = true;
      oneSeedButton.interactable = true;
      manySeedButton.interactable = true;

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
   /// able to pick a tool or click a question image while the congrats
   /// celebration is up. Reset() is what turns interaction back on.
   /// </summary>
   private void LockInteraction()
   {
      foreach (var q in questions)
      {
         q.imageButton.interactable = false;
      }

      crossButton.interactable = false;
      oneSeedButton.interactable = false;
      manySeedButton.interactable = false;
   }
}