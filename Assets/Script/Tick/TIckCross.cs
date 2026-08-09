using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TIckCross : MonoBehaviour
{
   [SerializeField] List<TickOrCross> questions;
   public int totalItem;
   int count;

   [System.Serializable]
   public class TickOrCross
   {
      public Button tickButton;
      public Button crossButton;
      public bool isTick;
   }

   void Start()
   {
      foreach (var tick in questions)
      {
         tick.tickButton.onClick.AddListener(() => Tick(tick));
         tick.crossButton.onClick.AddListener(() => Cross(tick));
      }
   }

   public void Tick(TickOrCross tick)
   {
      if (tick.isTick)
      {
         count++;

         // Disable the wrong button GameObject
         tick.crossButton.gameObject.SetActive(false);

         if (count == totalItem)
            EventManager.GameComplete();

         AudioManager.audioManager.Play("correct");
      }else
      {
         EventManager.WrongAnswer();
         AudioManager.audioManager.Play("wrong");
      }
   }

   public void Cross(TickOrCross cross)
   {
      if (!cross.isTick)
      {
         count++;

         // Disable the wrong button GameObject
         cross.tickButton.gameObject.SetActive(false);

         if (count == totalItem)
            EventManager.GameComplete();

         AudioManager.audioManager.Play("correct");
      }else
      {
         EventManager.WrongAnswer();
         AudioManager.audioManager.Play("wrong");
      }
   }

   public void Reset()
   {
      count = 0;

      foreach (var tick in questions)
      {
         // Enable both button GameObjects
         tick.tickButton.gameObject.SetActive(true);
         tick.crossButton.gameObject.SetActive(true);
      }

      AudioManager.audioManager.Play("button");
   }

   private void OnEnable()
   {
      count = 0;

      foreach (var tick in questions)
      {
         // Enable both button GameObjects
         tick.tickButton.gameObject.SetActive(true);
         tick.crossButton.gameObject.SetActive(true);
      }
   }
}