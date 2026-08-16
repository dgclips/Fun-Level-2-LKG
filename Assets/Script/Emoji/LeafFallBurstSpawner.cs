using UnityEngine;

/// <summary>
/// Spawns a staggered burst of falling-leaf UI images as a celebration effect,
/// using one or more leaf sprites at random. Replaces a real ParticleSystem
/// for this purpose, since Screen Space - Overlay canvases (used for the rest
/// of this game's UI) never render ParticleSystemRenderer content through a
/// Camera - only genuine UI Graphic components composite correctly there.
/// </summary>
public class LeafFallBurstSpawner : MonoBehaviour
{
   [SerializeField] private FallingLeaf leafPrefab;
   [SerializeField] private Sprite[] leafSprites;

   [SerializeField] private RectTransform spawnArea;

   [Tooltip("How far above the top of the spawn area the 'falling in' leaves start.")]
   [SerializeField] private float spawnHeightAboveTop = 80f;

   [Header("Burst")]
   [SerializeField] private int burstCount = 40;
   [SerializeField] private float staggerDelay = 0.03f;

   [Tooltip("Fraction of the burst that starts already scattered across the whole screen (immediate full coverage) instead of falling in from the top edge.")]
   [Range(0f, 1f)]
   [SerializeField] private float fullScreenFraction = 0.6f;


   void OnEnable()
   {
     // SpawnBurst();
   }
   public void SpawnBurst()
   {
      if (leafPrefab == null || spawnArea == null)
         return;

      int fullScreenCount = Mathf.RoundToInt(burstCount * fullScreenFraction);

      for (int i = 0; i < burstCount; i++)
      {
         bool spawnAcrossFullScreen = i < fullScreenCount;
         float delay = i * staggerDelay;

         if (delay <= 0f)
         {
            SpawnOne(spawnAcrossFullScreen);
         }
         else
         {
            // Capture the flag for the delayed call.
            bool captured = spawnAcrossFullScreen;
            StartCoroutine(SpawnOneDelayed(delay, captured));
         }
      }
   }

   private System.Collections.IEnumerator SpawnOneDelayed(float delay, bool spawnAcrossFullScreen)
   {
      yield return new WaitForSeconds(delay);
      SpawnOne(spawnAcrossFullScreen);
   }

   private void SpawnOne(bool spawnAcrossFullScreen)
   {
      FallingLeaf leaf = Instantiate(leafPrefab, spawnArea);

      RectTransform rect = leaf.GetComponent<RectTransform>();

      float halfWidth = spawnArea.rect.width * 0.5f;
      float halfHeight = spawnArea.rect.height * 0.5f;

      float x = Random.Range(-halfWidth, halfWidth);
      float y;

      if (spawnAcrossFullScreen)
      {
         // Scatter anywhere in the visible area for immediate full-screen coverage.
         y = Random.Range(-halfHeight, halfHeight);
      }
      else
      {
         // Start just above the top edge so it visibly falls into view.
         y = halfHeight + spawnHeightAboveTop;
      }

      rect.anchoredPosition = new Vector2(x, y);

      if (leafSprites != null && leafSprites.Length > 0)
      {
         var image = leaf.GetComponent<UnityEngine.UI.Image>();

         if (image != null)
         {
            image.sprite = leafSprites[Random.Range(0, leafSprites.Length)];
         }
      }
   }
}
