using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class FloatingEmoji : MonoBehaviour
{
   [Header("Movement")]
   [SerializeField] private float moveSpeed = 250f;
   [SerializeField] private float horizontalSpeed = 60f;

   [Header("Animation")]
   [SerializeField] private float duration = 2f;
   [SerializeField] private float startScale = 0.2f;
   [SerializeField] private float endScale = 1f;

   private RectTransform rect;
   private CanvasGroup canvasGroup;

   private Vector2 direction;
   private float timer;

   void Awake()
   {
      rect = GetComponent<RectTransform>();
      canvasGroup = GetComponent<CanvasGroup>();

      direction = new Vector2(
          Random.Range(-0.4f, 0.4f),
          1f).normalized;

      transform.localScale = Vector3.one * startScale;
      canvasGroup.alpha = 1;
   }

   void Update()
   {
      timer += Time.deltaTime;

      rect.anchoredPosition += new Vector2(
          direction.x * horizontalSpeed,
          direction.y * moveSpeed) * Time.deltaTime;

      float t = timer / duration;

      transform.localScale = Vector3.Lerp(
          Vector3.one * startScale,
          Vector3.one * endScale,
          t);

      canvasGroup.alpha = Mathf.Lerp(1, 0, t);

      if (timer >= duration)
         Destroy(gameObject);
   }
}