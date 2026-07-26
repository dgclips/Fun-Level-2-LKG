using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCompleted : MonoBehaviour
{
    public GameObject congrateImage;
   [SerializeField] private SadEmojiSpawner emojiSpawner;
   void OnEnable()
    { 
       EventManager.OnComplete += Showed;
      EventManager.wrong += Wrong;
    }
    void OnDisable()
    {
       EventManager.OnComplete -= Showed;
      EventManager.wrong -= Wrong;
    }
    
    void Showed()
    {
      Show();
    }

    void Show()
    {
        congrateImage.SetActive(true);
        Invoke("Hide", 10f);
    }

    void Hide()
    {
        congrateImage.SetActive(false);
    }

    void Wrong()
   {
      emojiSpawner.SpawnSadEmoji();
   }
}
