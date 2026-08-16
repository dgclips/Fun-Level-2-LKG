using UnityEngine;

/// <summary>
/// Spawns a small burst of floating "happy" emoji (stars, hearts, confetti - whatever
/// sprite is set on the prefab) as a positive celebration effect. Mirrors
/// SadEmojiSpawner's single-spawn pattern, but fires several at once with a
/// slight stagger so it reads as a little celebration rather than one icon.
/// </summary>
public class HappyEmojiBurstSpawner : MonoBehaviour
{
   [SerializeField] private FloatingEmoji emojiPrefab;

   [SerializeField] private RectTransform spawnArea;

   [SerializeField] private float randomX = 200f;

   [Header("Burst")]
   [SerializeField] private int burstCount = 6;
   [SerializeField] private float staggerDelay = 0.06f;

   public void SpawnBurst()
   {
      if (emojiPrefab == null || spawnArea == null)
         return;

      for (int i = 0; i < burstCount; i++)
      {
         float delay = i * staggerDelay;

         if (delay <= 0f)
         {
            SpawnOne();
         }
         else
         {
            Invoke(nameof(SpawnOne), delay);
         }
      }
   }

   private void SpawnOne()
   {
      FloatingEmoji emoji = Instantiate(emojiPrefab, spawnArea);

      RectTransform rect = emoji.GetComponent<RectTransform>();

      rect.anchoredPosition = new Vector2(
          Random.Range(-randomX, randomX),
          0);
   }
}
