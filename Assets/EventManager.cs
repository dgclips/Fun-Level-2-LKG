using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EventManager : MonoBehaviour
{
    public static event Action OnComplete;
   public static event Action wrong;
   public static event Action OnActivityClosed;

    public static void GameComplete()
    {
        OnComplete?.Invoke();
    }

   public static void WrongAnswer()
   {
      wrong?.Invoke();
   }

   // Fired when the player closes the current activity (close button ->
   // LearningPageButtonSpawner.DisableAllPage) so anything still celebrating
   // (e.g. the congrats popup/particles) can cut itself off immediately
   // instead of lingering on top of the page-selection UI.
   public static void ActivityClosed()
   {
      OnActivityClosed?.Invoke();
   }
}
