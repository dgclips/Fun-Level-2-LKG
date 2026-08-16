using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A single UI-Image "leaf" that drifts and rotates downward before fading out
/// and destroying itself. Pure UI Graphic (no ParticleSystem/Renderer) so it
/// renders correctly inside a Screen Space - Overlay canvas, on top of the
/// rest of the game's UI - unlike a real ParticleSystem, which Overlay
/// canvases never draw through a Camera.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class FallingLeaf : MonoBehaviour
{
   [Header("Movement")]
   [SerializeField] private float fallSpeed = 220f;
   [SerializeField] private float driftSpeed = 40f;
   [SerializeField] private float spinSpeed = 90f;

   [Header("Animation")]
   [SerializeField] private float duration = 5f;
   [SerializeField] private float startScale = 0.4f;
   [SerializeField] private float endScale = 1f;

   private RectTransform rect;
   private CanvasGroup canvasGroup;

   private float driftDirection;
   private float spinDirection;
   private float timer;

   void Awake()
   {
      rect = GetComponent<RectTransform>();
      canvasGroup = GetComponent<CanvasGroup>();

      driftDirection = Random.Range(-1f, 1f);
      spinDirection = Random.value < 0.5f ? -1f : 1f;

      transform.localScale = Vector3.one * startScale;
      canvasGroup.alpha = 1f;
   }

   void Update()
   {
      timer += Time.unscaledDeltaTime;

      rect.anchoredPosition += new Vector2(
          driftDirection * driftSpeed,
          -fallSpeed) * Time.unscaledDeltaTime;

      transform.Rotate(0, 0, spinDirection * spinSpeed * Time.unscaledDeltaTime);

      float t = timer / duration;

      transform.localScale = Vector3.Lerp(
          Vector3.one * startScale,
          Vector3.one * endScale,
          t);

      canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

      if (timer >= duration)
         Destroy(gameObject);
   }
}
