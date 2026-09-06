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

   /// <summary>
   /// Cancels any still-pending staggered spawns and immediately removes every
   /// emoji already spawned by this burst - used when the celebration is cut
   /// short (e.g. the player closes the activity) so nothing lingers on screen.
   /// </summary>
   public void ClearBurst()
   {
      CancelInvoke(nameof(SpawnOne));

      if (spawnArea == null)
         return;

      for (int i = spawnArea.childCount - 1; i >= 0; i--)
      {
         Destroy(spawnArea.GetChild(i).gameObject);
      }
   }
}
