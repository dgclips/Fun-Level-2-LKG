using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace GameBook.UI
{
    /// <summary>
    /// Animates the title screen's Start button and the flow it triggers -
    /// punches the button on click, then fades/pops the splash cover out and
    /// the page-selection screen in, instead of an instant SetActive swap.
    /// Mirrors the ShowPanel/HidePanel/PunchButton style used by
    /// LearningPageButtonSpawner so the whole app's transitions feel consistent.
    /// </summary>
    public class StartScreenController : MonoBehaviour
    {
        [Header("Start Button")]
        [SerializeField] private Button startButton;

        [Header("Flow")]
        [Tooltip("Splash cover shown before Start is pressed - faded out on click.")]
        [SerializeField] private GameObject firstPage;

        [Tooltip("Panels revealed after Start is pressed (Main background, page list, ...).")]
        [SerializeField] private GameObject[] revealPanels;

        [Header("Animation")]
        [SerializeField] private float fadeDuration = 0.25f;
        [SerializeField] private Ease popEase = Ease.OutBack;

        private void OnEnable()
        {
            if (startButton != null)
                startButton.onClick.AddListener(OnStartClicked);
        }

        private void OnDisable()
        {
            if (startButton != null)
                startButton.onClick.RemoveListener(OnStartClicked);
        }

        private void OnStartClicked()
        {
            AudioManager.audioManager.Play("button");

            HidePanel(firstPage);

            foreach (GameObject panel in revealPanels)
            {
                ShowPanel(panel);
            }
        }

        // ------------------------------------------------------------
        //  ANIMATION HELPERS
        // ------------------------------------------------------------

        private CanvasGroup GetCanvasGroup(GameObject target)
        {
            CanvasGroup group = target.GetComponent<CanvasGroup>();

            if (group == null)
            {
                group = target.AddComponent<CanvasGroup>();
            }

            return group;
        }

        /// <summary>
        /// Fades and scales a panel in. Activates it first.
        /// </summary>
        private void ShowPanel(GameObject panel)
        {
            if (panel == null)
            {
                return;
            }

            panel.SetActive(true);

            CanvasGroup group = GetCanvasGroup(panel);

            group.DOKill();
            panel.transform.DOKill();

            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            group.DOFade(1f, fadeDuration)
                 .SetEase(Ease.OutQuad)
                 .SetUpdate(true)
                 .SetLink(panel)
                 .OnComplete(() =>
                 {
                    group.interactable = true;
                    group.blocksRaycasts = true;
                 });

            panel.transform.localScale = Vector3.one * 0.94f;

            panel.transform.DOScale(1f, fadeDuration)
                 .SetEase(popEase)
                 .SetUpdate(true)
                 .SetLink(panel);
        }

        /// <summary>
        /// Fades a panel out, then deactivates it.
        /// </summary>
        private void HidePanel(GameObject panel)
        {
            if (panel == null || !panel.activeSelf)
            {
                return;
            }

            CanvasGroup group = GetCanvasGroup(panel);

            group.DOKill();

            group.interactable = false;
            group.blocksRaycasts = false;

            group.DOFade(0f, fadeDuration)
                 .SetEase(Ease.InQuad)
                 .SetUpdate(true)
                 .SetLink(panel)
                 .OnComplete(() => panel.SetActive(false));
        }

        private void OnDestroy()
        {
            DOTween.Kill(transform);
        }
    }
}
