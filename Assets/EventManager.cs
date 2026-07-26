using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EventManager : MonoBehaviour
{
    public static event Action OnComplete;
   public static event Action wrong;

    public static void GameComplete()
    {
        OnComplete?.Invoke();
    }

   public static void WrongAnswer()
   {
      wrong?.Invoke();
   }
}
