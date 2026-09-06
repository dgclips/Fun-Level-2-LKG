using UnityEngine;
using UnityEngine.Video;
using DG.Tweening;

namespace GameBook.UI
{
    /// <summary>
    /// Plays the intro video before the title screen every time the app is
    /// opened, then fades/pops into the existing Start screen (first page +
    /// Start button). If no clip has been assigned yet, the video is skipped
    /// and the Start screen shows immediately instead of getting stuck on a
    /// blank video.
    /// BGM only ever starts here (AudioManager no longer auto-plays it), so
    /// it never plays underneath the video - only once it's done, or right
    /// away when there's no video to show.
    /// </summary>
    public class IntroVideoController : MonoBehaviour
    {
        [Header("Video")]
        [SerializeField] private VideoPlayer videoPlayer;

        [Tooltip("Full-screen panel that hosts the video's RawImage - shown while the video plays.")]
        [SerializeField] private GameObject videoScreen;

        [Header("Start Screen")]
        [Tooltip("Splash cover shown once the video is done - fades/pops in.")]
        [SerializeField] private GameObject firstPage;

        [Tooltip("The Start button itself - just activated here; it animates its own idle/select states.")]
        [SerializeField] private GameObject startButtonObject;

        [Header("Animation")]
        [SerializeField] private float fadeDuration = 0.35f;
        [SerializeField] private Ease popEase = Ease.OutBack;

        private void Start()
        {
            if (videoPlayer == null || videoPlayer.clip == null)
            {
                // No clip assigned yet - go straight to the Start screen
                // instead of getting stuck on a blank video.
                if (videoScreen != null)
                {
                    videoScreen.SetActive(false);
                }

                ShowStartScreenInstant();

                AudioManager.audioManager.Play("bg");
                return;
            }

            if (firstPage != null)
            {
                firstPage.SetActive(false);
            }

            if (startButtonObject != null)
            {
                startButtonObject.SetActive(false);
            }

            videoScreen.SetActive(true);

            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.Play();
        }

        private void OnVideoFinished(VideoPlayer vp)
        {
            vp.loopPointReached -= OnVideoFinished;

            HidePanel(videoScreen);

            ShowPanel(firstPage);

            if (startButtonObject != null)
            {
                startButtonObject.SetActive(true);
            }

            AudioManager.audioManager.Play("bg");
        }

        private void ShowStartScreenInstant()
        {
            if (firstPage != null)
            {
                firstPage.SetActive(true);
            }

            if (startButtonObject != null)
            {
                startButtonObject.SetActive(true);
            }
        }

        // ------------------------------------------------------------
        //  ANIMATION HELPERS (mirrors StartScreenController)
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
            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached -= OnVideoFinished;
            }

            DOTween.Kill(transform);
        }
    }
}
