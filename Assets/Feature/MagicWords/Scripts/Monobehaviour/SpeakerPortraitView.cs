using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MagicWords.Monobehaviour
{
    /// <summary>
    /// One of the two large speaker portraits standing behind the dialogue boxes. It owns no
    /// state - who is on this side, and whether they are the one talking, is decided by the
    /// sequence and pushed in.
    ///
    /// The missing-avatar look is a designed state, not a spinner: the endpoint ships a speaker
    /// with no avatar entry at all and two URLs that cannot resolve, so it is on screen on a
    /// normal run and has to read as intentional. It uses the letter-avatar convention every
    /// contacts app uses - the speaker's own initial on a tinted plate, plus a NO AVATAR tag -
    /// which is visibly derived from the data and cannot be mistaken for a picture still loading.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class SpeakerPortraitView : MonoBehaviour
    {
        [SerializeField] private Image avatarImage;
        [SerializeField] private Image plateImage;
        [SerializeField] private Color plateColor = Color.white;
        [SerializeField] private Color missingAvatarPlateColor = Color.white;
        [SerializeField] private TMP_Text monogram;
        [SerializeField] private GameObject missingAvatarTag;
        [SerializeField] private GameObject speakingGlow;

        [Header("Speaking / listening")]
        [Tooltip("Alpha of the portrait that is not speaking. Fading toward the background reads "
                 + "as 'stepped back'; a colour multiply just makes the character look repainted.")]
        [SerializeField] private float listeningAlpha = 0.62f;
        [SerializeField] private float listeningScale = 0.85f;

        private CanvasGroup _canvasGroup;
        private CanvasGroup CanvasGroup => _canvasGroup ??= GetComponent<CanvasGroup>();

        public void Initialize()
        {
            Hide();
        }

        public void Bind(string speakerName, Sprite avatarSprite)
        {
            var hasAvatar = avatarSprite != null;
            avatarImage.sprite = avatarSprite;
            avatarImage.enabled = hasAvatar;
            plateImage.color = hasAvatar ? plateColor : missingAvatarPlateColor;
            monogram.text = SpeakerInitial.Of(speakerName);
            monogram.gameObject.SetActive(!hasAvatar);
            missingAvatarTag.SetActive(!hasAvatar);
            gameObject.SetActive(true);
        }

        public void SetSpeaking(bool speaking)
        {
            CanvasGroup.alpha = speaking ? 1f : listeningAlpha;
            transform.localScale = Vector3.one * (speaking ? 1f : listeningScale);
            speakingGlow.SetActive(speaking);

            // On a narrow canvas the two portraits can overlap; the speaker always wins.
            if (speaking)
                transform.SetAsLastSibling();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
