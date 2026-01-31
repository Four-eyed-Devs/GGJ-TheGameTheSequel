using System.Collections;
using UnityEngine;

namespace Interrogation.Dialogue
{
    /// <summary>
    /// Manages voice playback for investigator lines.
    /// Loads AudioClips from Resources and plays them with proper timing.
    /// </summary>
    public class VoiceManager : MonoBehaviour
    {
        public static VoiceManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource investigatorSource;
        [SerializeField] private AudioSource playerSource; // Optional for player voice

        [Header("Settings")]
        [SerializeField] private float defaultVolume = 1f;
        [SerializeField] private float fadeOutDuration = 0.5f;

        [Header("Debug")]
        [SerializeField] private bool logPlayback = true;

        public bool IsPlaying => investigatorSource != null && investigatorSource.isPlaying;
        public float CurrentClipLength => investigatorSource?.clip?.length ?? 0f;

        private Coroutine fadeCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Create audio source if not assigned
            if (investigatorSource == null)
            {
                investigatorSource = gameObject.AddComponent<AudioSource>();
                investigatorSource.playOnAwake = false;
                investigatorSource.volume = defaultVolume;
            }
        }

        /// <summary>
        /// Play an investigator voice line
        /// </summary>
        public void PlayInvestigatorLine(InvestigatorLine line)
        {
            if (line == null || string.IsNullOrEmpty(line.voiceClipPath))
            {
                if (logPlayback)
                    Debug.Log("[VoiceManager] No voice clip for line, skipping audio");
                return;
            }

            AudioClip clip = DialogueLoader.LoadVoiceClip(line.voiceClipPath);
            if (clip != null)
            {
                PlayClip(investigatorSource, clip);
            }
            else
            {
                Debug.LogWarning($"[VoiceManager] Failed to load voice clip: {line.voiceClipPath}");
            }
        }

        /// <summary>
        /// Play an audio clip on the investigator source
        /// </summary>
        public void PlayClip(AudioClip clip)
        {
            PlayClip(investigatorSource, clip);
        }

        /// <summary>
        /// Play an audio clip on a specific source
        /// </summary>
        private void PlayClip(AudioSource source, AudioClip clip)
        {
            if (source == null || clip == null) return;

            // Stop any current playback
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            source.Stop();

            source.clip = clip;
            source.volume = defaultVolume;
            source.Play();

            if (logPlayback)
                Debug.Log($"[VoiceManager] Playing clip: {clip.name} ({clip.length:F2}s)");
        }

        /// <summary>
        /// Stop current playback immediately
        /// </summary>
        public void Stop()
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }

            investigatorSource?.Stop();
            playerSource?.Stop();
        }

        /// <summary>
        /// Fade out current playback
        /// </summary>
        public void FadeOut()
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeOutCoroutine(investigatorSource));
        }

        /// <summary>
        /// Wait for current clip to finish playing
        /// </summary>
        public IEnumerator WaitForClipEnd()
        {
            while (investigatorSource != null && investigatorSource.isPlaying)
            {
                yield return null;
            }
        }

        /// <summary>
        /// Wait for a specific duration or until clip ends, whichever comes first
        /// </summary>
        public IEnumerator WaitForDuration(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration && investigatorSource != null && investigatorSource.isPlaying)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        /// <summary>
        /// Play a line and wait for it to complete
        /// </summary>
        public IEnumerator PlayAndWait(InvestigatorLine line)
        {
            PlayInvestigatorLine(line);

            if (investigatorSource != null && investigatorSource.clip != null)
            {
                yield return WaitForClipEnd();
            }
        }

        /// <summary>
        /// Get estimated duration for a line (clip length or text-based estimate)
        /// </summary>
        public float GetLineDuration(InvestigatorLine line, float wordsPerMinute = 150f)
        {
            if (line == null) return 0f;

            // If we have a voice clip, use its length
            if (!string.IsNullOrEmpty(line.voiceClipPath))
            {
                AudioClip clip = DialogueLoader.LoadVoiceClip(line.voiceClipPath);
                if (clip != null)
                {
                    return clip.length;
                }
            }

            // Otherwise estimate based on text length
            if (!string.IsNullOrEmpty(line.text))
            {
                int wordCount = line.text.Split(' ', System.StringSplitOptions.RemoveEmptyEntries).Length;
                return (wordCount / wordsPerMinute) * 60f;
            }

            return 2f; // Default fallback
        }

        private IEnumerator FadeOutCoroutine(AudioSource source)
        {
            if (source == null) yield break;

            float startVolume = source.volume;
            float elapsed = 0f;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeOutDuration);
                yield return null;
            }

            source.Stop();
            source.volume = defaultVolume;
            fadeCoroutine = null;
        }

        /// <summary>
        /// Set master volume for voice playback
        /// </summary>
        public void SetVolume(float volume)
        {
            defaultVolume = Mathf.Clamp01(volume);
            if (investigatorSource != null && !investigatorSource.isPlaying)
            {
                investigatorSource.volume = defaultVolume;
            }
            if (playerSource != null && !playerSource.isPlaying)
            {
                playerSource.volume = defaultVolume;
            }
        }
    }
}
