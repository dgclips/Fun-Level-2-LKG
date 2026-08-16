using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GameCompleted : MonoBehaviour
{
    public GameObject congrateImage;
   [SerializeField] private SadEmojiSpawner emojiSpawner;
   [SerializeField] private HappyEmojiBurstSpawner happyEmojiSpawner;

   [Tooltip("Once the congrats/confetti celebration finishes, the player is sent back to this page's button/activity list.")]
   [SerializeField] private LearningPageButtonSpawner pageSpawner;

   [Tooltip("UI-based leaf confetti burst. Uses real UI Images (not a ParticleSystem) so it " +
            "renders correctly above the Screen Space - Overlay UI.")]
   [SerializeField] private LeafFallBurstSpawner leafFallSpawner;

   [Header("Celebration Animation")]
   [SerializeField] private float popInDuration = 0.5f;
   [SerializeField] private Ease popInEase = Ease.OutBack;
   [SerializeField] private float wiggleAngle = 6f;

   void OnEnable()
    {
       EventManager.OnComplete += Showed;
      EventManager.wrong += Wrong;
    }
    void OnDisable()
    {
       EventManager.OnComplete -= Showed;
      EventManager.wrong -= Wrong;

      // congrateImage may already be destroyed by the time this fires
      // (e.g. during scene teardown), so guard before touching it.
      if (congrateImage != null)
         congrateImage.transform.DOKill();
    }

    void Showed()
    {
      Show();
    }

    void Show()
    {
        congrateImage.SetActive(true);

        // Positive, bouncy pop-in with a little happy wiggle.
        Transform t = congrateImage.transform;
        t.DOKill();
        t.localScale = Vector3.zero;
        t.localRotation = Quaternion.identity;

        Sequence celebration = DOTween.Sequence()
            .SetUpdate(true)
            .SetLink(congrateImage);

        celebration.Append(t.DOScale(1f, popInDuration).SetEase(popInEase));
        celebration.Join(
            t.DOPunchRotation(new Vector3(0, 0, wiggleAngle), popInDuration * 1.4f, 8, 0.9f)
        );

        happyEmojiSpawner?.SpawnBurst();
        leafFallSpawner?.SpawnBurst();

        Invoke("Hide", 10f);
    }

    void Hide()
    {
        if (congrateImage == null)
            return;

        congrateImage.transform.DOKill();
        congrateImage.SetActive(false);

        // Celebration is over - return the player to the page's button list.
        pageSpawner?.DisableAllPage();
    }

    void Wrong()
   {
      emojiSpawner.SpawnSadEmoji();
   }
}
