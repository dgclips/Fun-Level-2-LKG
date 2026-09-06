using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace GameBook.UI
{
    /// <summary>
    /// Idle and select (press) feedback for the title screen's Start button -
    /// a gentle breathing loop invites a tap while idle, and the button
    /// shrinks while pressed and springs back on release. The click itself
    /// and the screen flow it triggers stay in StartScreenController; this
    /// only owns the button's own idle/pressed look.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class StartButtonAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [Header("Idle Animation")]
        [Tooltip("How much the button grows at the peak of the idle breathing loop.")]
        [SerializeField] private float idleScale = 1.06f;
        [SerializeField] private float idleDuration = 0.7f;

        [Header("Select (Press) Animation")]
        [Tooltip("How much the button shrinks while pressed/selected.")]
        [SerializeField] private float pressScale = 0.9f;
        [SerializeField] private float pressDuration = 0.12f;
        [SerializeField] private Ease releaseEase = Ease.OutBack;

        private Button _button;
        private RectTransform _rect;
        private Vector3 _baseScale;
        private Tween _idleTween;
        private bool _pressed;

        // Idle only ever plays before the button's first press - once the
        // player has engaged with it there's nothing left to invite a tap for.
        private bool _hasInteracted;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _rect = GetComponent<RectTransform>();
            _baseScale = _rect.localScale;
        }

        private void OnEnable()
        {
            PlayIdle();
        }

        private void OnDisable()
        {
            _idleTween?.Kill();
            _idleTween = null;
            _rect.DOKill();
            _rect.localScale = _baseScale;
            _pressed = false;
        }

        private void PlayIdle()
        {
            if (_hasInteracted)
            {
                return;
            }

            _rect.localScale = _baseScale;

            _idleTween = _rect.DOScale(_baseScale * idleScale, idleDuration)
                               .SetEase(Ease.InOutSine)
                               .SetLoops(-1, LoopType.Yoyo)
                               .SetUpdate(true)
                               .SetLink(gameObject);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_button.interactable)
            {
                return;
            }

            _pressed = true;
            _hasInteracted = true;

            _idleTween?.Kill();
            _idleTween = null;

            _rect.DOKill();

            _rect.DOScale(_baseScale * pressScale, pressDuration)
                 .SetEase(Ease.OutQuad)
                 .SetUpdate(true)
                 .SetLink(gameObject);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ReleasePress();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ReleasePress();
        }

        private void ReleasePress()
        {
            if (!_pressed)
            {
                return;
            }

            _pressed = false;

            _rect.DOKill();

            _rect.DOScale(_baseScale, pressDuration)
                 .SetEase(releaseEase)
                 .SetUpdate(true)
                 .SetLink(gameObject);
        }
    }
}
