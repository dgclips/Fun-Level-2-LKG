using System.Collections;
using UnityEngine;

public class SadEmojiSpawner : MonoBehaviour
{
   [SerializeField] private FloatingEmoji emojiPrefab;

   [SerializeField] private RectTransform spawnArea;

   [SerializeField] private float randomX = 100f;

   public void SpawnSadEmoji()
   {
      FloatingEmoji emoji = Instantiate(emojiPrefab, spawnArea);

      RectTransform rect = emoji.GetComponent<RectTransform>();

      rect.anchoredPosition = new Vector2(
          Random.Range(-randomX, randomX),
          0);
   }
}