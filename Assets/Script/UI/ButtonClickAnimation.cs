using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GameBook.UI
{
    /// <summary>
    /// Plays a quick punch-scale animation whenever the attached Button is clicked.
    /// Attach to any UI Button (e.g. the back button) - no extra setup required.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ButtonClickAnimation : MonoBehaviour
    {
        [Tooltip("How much the button scales down/up during the click animation.")]
        [SerializeField] private float punchScale = 0.85f;

        [Tooltip("Total duration of the click animation in seconds.")]
        [SerializeField] private float duration = 0.15f;

        private Button _button;
        private RectTransform _rectTransform;
        private Vector3 _originalScale;
        private Coroutine _animCoroutine;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _rectTransform = GetComponent<RectTransform>();
            _originalScale = _rectTransform.localScale;
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(PlayClickAnimation);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(PlayClickAnimation);
            if (_animCoroutine != null)
            {
                StopCoroutine(_animCoroutine);
                _animCoroutine = null;
            }
            _rectTransform.localScale = _originalScale;
        }

        private void PlayClickAnimation()
        {
         AudioManager.audioManager.Play("button");
         if (_animCoroutine != null)
                StopCoroutine(_animCoroutine);

            _animCoroutine = StartCoroutine(AnimateClick());
        }

        private IEnumerator AnimateClick()
        {
            float halfDuration = duration * 0.5f;
            Vector3 shrunkScale = _originalScale * punchScale;

            // Scale down
            float t = 0f;
            while (t < halfDuration)
            {
                t += Time.unscaledDeltaTime;
                float lerp = Mathf.Clamp01(t / halfDuration);
                _rectTransform.localScale = Vector3.Lerp(_originalScale, shrunkScale, lerp);
                yield return null;
            }

            // Scale back up
            t = 0f;
            while (t < halfDuration)
            {
                t += Time.unscaledDeltaTime;
                float lerp = Mathf.Clamp01(t / halfDuration);
                _rectTransform.localScale = Vector3.Lerp(shrunkScale, _originalScale, lerp);
                yield return null;
            }

            _rectTransform.localScale = _originalScale;
            _animCoroutine = null;
        }
    }
}
