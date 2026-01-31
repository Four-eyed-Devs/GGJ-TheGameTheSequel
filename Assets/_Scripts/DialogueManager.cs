using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrogation.Dialogue
{
    /// <summary>
    /// Game state phases
    /// </summary>
    public enum GamePhase
    {
        Intro,
        Question,
        WaitingForMask,
        PlayerResponse,
        InvestigatorReaction,
        Ending,
        GameOver
    }

    /// <summary>
    /// Main dialogue flow controller.
    /// Manages the interrogation sequence: intro -> questions (1-6) -> ending
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private int totalQuestions = 6;
        [SerializeField] private int maxMaskUses = 2;
        [SerializeField] private float delayBetweenLines = 1f;
        [SerializeField] private float delayBeforeMaskSelection = 0.5f;

        [Header("Debug")]
        [SerializeField] private GamePhase currentPhase;
        [SerializeField] private int currentQuestionIndex;
        [SerializeField] private bool autoStart = true;

        // Events
        public event Action<GamePhase> OnPhaseChanged;
        public event Action<int> OnQuestionStarted;
        public event Action<bool> OnGameEnded; // true = good ending
        public event Action OnMaskSelectionEnabled;
        public event Action OnMaskSelectionDisabled;

        // Data
        private IntroData introData;
        private EndingsData endingsData;
        private QuestionData currentQuestion;
        private Dictionary<MaskType, int> maskUseCount = new Dictionary<MaskType, int>();

        // State
        private bool isProcessing;
        private bool waitingForMaskSelection;

        public GamePhase CurrentPhase => currentPhase;
        public int CurrentQuestionIndex => currentQuestionIndex;
        public bool IsWaitingForMask => waitingForMaskSelection;
        public int TotalQuestions => totalQuestions;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializeMaskUseCount();
        }

        private void Start()
        {
            if (autoStart)
            {
                StartGame();
            }
        }

        /// <summary>
        /// Initialize mask use tracking
        /// </summary>
        private void InitializeMaskUseCount()
        {
            maskUseCount.Clear();
            foreach (MaskType mask in Enum.GetValues(typeof(MaskType)))
            {
                maskUseCount[mask] = 0;
            }
        }

        /// <summary>
        /// Start the interrogation game
        /// </summary>
        public void StartGame()
        {
            Debug.Log("[DialogueManager] Starting interrogation...");

            InitializeMaskUseCount();
            currentQuestionIndex = 0;

            // Load dialogue data
            introData = DialogueLoader.LoadIntro();
            endingsData = DialogueLoader.LoadEndings();

            if (introData == null)
            {
                Debug.LogError("[DialogueManager] Failed to load intro data!");
                return;
            }

            StartCoroutine(GameFlow());
        }

        /// <summary>
        /// Main game flow coroutine
        /// </summary>
        private IEnumerator GameFlow()
        {
            // Phase 1: Intro
            yield return PlayIntro();

            // Phase 2: Questions (1-6)
            for (int i = 1; i <= totalQuestions; i++)
            {
                currentQuestionIndex = i;
                yield return PlayQuestion(i);
            }

            // Phase 3: Ending
            yield return PlayEnding();
        }

        /// <summary>
        /// Play the intro sequence
        /// </summary>
        private IEnumerator PlayIntro()
        {
            SetPhase(GamePhase.Intro);

            if (introData?.introVariants == null || introData.introVariants.Length == 0)
            {
                Debug.LogWarning("[DialogueManager] No intro variants found");
                yield break;
            }

            // Pick random intro variant
            int variantIndex = UnityEngine.Random.Range(0, introData.introVariants.Length);
            InvestigatorLine introLine = introData.introVariants[variantIndex];

            yield return PlayInvestigatorLine(introLine);
            yield return new WaitForSeconds(delayBetweenLines);
        }

        /// <summary>
        /// Play a question sequence
        /// </summary>
        private IEnumerator PlayQuestion(int questionNumber)
        {
            SetPhase(GamePhase.Question);
            OnQuestionStarted?.Invoke(questionNumber);

            // Load question data
            currentQuestion = DialogueLoader.LoadQuestion(questionNumber);
            if (currentQuestion == null)
            {
                Debug.LogError($"[DialogueManager] Failed to load question {questionNumber}");
                yield break;
            }

            Debug.Log($"[DialogueManager] Question {questionNumber}: Correct mask is {currentQuestion.correctMask}");

            // Play investigator question
            yield return PlayInvestigatorLine(currentQuestion.investigatorQuestion);
            yield return new WaitForSeconds(delayBeforeMaskSelection);

            // Wait for mask selection
            SetPhase(GamePhase.WaitingForMask);
            waitingForMaskSelection = true;
            OnMaskSelectionEnabled?.Invoke();

            // Wait until a mask is selected
            while (waitingForMaskSelection)
            {
                yield return null;
            }
            
            // Wait for the mask selection processing to complete (player response + investigator reaction)
            while (isProcessing)
            {
                yield return null;
            }
        }

        /// <summary>
        /// Called by MaskCard3D when player selects a mask
        /// </summary>
        public void OnMaskSelected(MaskType selectedMask)
        {
            if (!waitingForMaskSelection)
            {
                Debug.LogWarning("[DialogueManager] Mask selected but not waiting for selection");
                return;
            }

            StartCoroutine(ProcessMaskSelection(selectedMask));
        }

        /// <summary>
        /// Process the player's mask selection
        /// </summary>
        private IEnumerator ProcessMaskSelection(MaskType selectedMask)
        {
            isProcessing = true;
            waitingForMaskSelection = false;
            OnMaskSelectionDisabled?.Invoke();

            // Find the mask answer
            MaskAnswer selectedAnswer = FindMaskAnswer(selectedMask);
            if (selectedAnswer == null)
            {
                Debug.LogError($"[DialogueManager] No answer found for mask: {selectedMask}");
                isProcessing = false;
                yield break;
            }

            // Increment mask use count
            maskUseCount[selectedMask]++;
            Debug.Log($"[DialogueManager] Mask {selectedMask} used {maskUseCount[selectedMask]}/{maxMaskUses} times");

            // Update tension based on correct/wrong
            bool isCorrect = selectedAnswer.isCorrect;
            if (isCorrect)
            {
                TensionMeter.Instance?.OnCorrectMask();
            }
            else
            {
                TensionMeter.Instance?.OnWrongMask();
            }

            // Phase: Player response
            SetPhase(GamePhase.PlayerResponse);
            yield return PlayPlayerLine(selectedAnswer.playerLine);
            yield return new WaitForSeconds(delayBetweenLines);

            // Phase: Investigator reaction
            SetPhase(GamePhase.InvestigatorReaction);
            InvestigatorLine reaction = GetReactionLine(selectedAnswer);
            if (reaction != null)
            {
                yield return PlayInvestigatorLine(reaction);
                yield return new WaitForSeconds(delayBetweenLines);
            }
            
            isProcessing = false;
        }

        /// <summary>
        /// Find the mask answer in current question
        /// </summary>
        private MaskAnswer FindMaskAnswer(MaskType maskType)
        {
            if (currentQuestion?.maskAnswers == null) return null;

            string maskName = maskType.ToString();
            foreach (var answer in currentQuestion.maskAnswers)
            {
                if (answer.maskType.Equals(maskName, StringComparison.OrdinalIgnoreCase))
                {
                    return answer;
                }
            }
            return null;
        }

        /// <summary>
        /// Get appropriate reaction line based on tension
        /// </summary>
        private InvestigatorLine GetReactionLine(MaskAnswer answer)
        {
            if (answer?.reactions == null) return null;

            bool isHighTension = TensionMeter.Instance?.IsHighTension ?? true;
            InvestigatorLine[] reactions = isHighTension 
                ? answer.reactions.highTension 
                : answer.reactions.lowTension;

            if (reactions == null || reactions.Length == 0) return null;

            // Pick random reaction from the appropriate tension level
            int index = UnityEngine.Random.Range(0, reactions.Length);
            return reactions[index];
        }

        /// <summary>
        /// Play the ending sequence
        /// </summary>
        private IEnumerator PlayEnding()
        {
            SetPhase(GamePhase.Ending);

            string endingType = TensionMeter.Instance?.GetEndingType() ?? "bad";
            bool isGoodEnding = endingType == "good";

            Debug.Log($"[DialogueManager] Playing {endingType} ending");

            EndingData ending = isGoodEnding ? endingsData?.goodEnding : endingsData?.badEnding;
            if (ending?.lines != null)
            {
                foreach (var line in ending.lines)
                {
                    yield return PlayInvestigatorLine(line);
                    yield return new WaitForSeconds(delayBetweenLines);
                }
            }

            SetPhase(GamePhase.GameOver);
            OnGameEnded?.Invoke(isGoodEnding);
        }

        /// <summary>
        /// Play an investigator line with subtitles and voice
        /// </summary>
        private IEnumerator PlayInvestigatorLine(InvestigatorLine line)
        {
            if (line == null) yield break;

            // Start voice playback
            VoiceManager.Instance?.PlayInvestigatorLine(line);

            // Show subtitle
            float duration = VoiceManager.Instance?.GetLineDuration(line) ?? 3f;
            SubtitleUI.Instance?.ShowInvestigatorLine(line.text, duration);

            // Wait for typewriter effect
            if (SubtitleUI.Instance != null)
            {
                yield return SubtitleUI.Instance.WaitForTypewriter();
            }

            // Wait for voice to finish
            if (VoiceManager.Instance != null)
            {
                yield return VoiceManager.Instance.WaitForClipEnd();
            }
            else
            {
                // No voice, wait based on text length
                yield return new WaitForSeconds(duration);
            }
        }

        /// <summary>
        /// Play a player line (subtitle only, no voice)
        /// </summary>
        private IEnumerator PlayPlayerLine(string text)
        {
            if (string.IsNullOrEmpty(text)) yield break;

            // Estimate duration based on word count
            int wordCount = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            float duration = Mathf.Max(2f, wordCount * 0.3f);

            SubtitleUI.Instance?.ShowPlayerLine(text, duration);

            // Wait for typewriter
            if (SubtitleUI.Instance != null)
            {
                yield return SubtitleUI.Instance.WaitForTypewriter();
                yield return new WaitForSeconds(1f); // Brief pause after player speaks
            }
        }

        /// <summary>
        /// Set the current game phase
        /// </summary>
        private void SetPhase(GamePhase phase)
        {
            currentPhase = phase;
            Debug.Log($"[DialogueManager] Phase: {phase}");
            OnPhaseChanged?.Invoke(phase);
        }

        /// <summary>
        /// Check if a mask can still be used
        /// </summary>
        public bool CanUseMask(MaskType mask)
        {
            return maskUseCount.TryGetValue(mask, out int count) && count < maxMaskUses;
        }

        /// <summary>
        /// Get remaining uses for a mask
        /// </summary>
        public int GetMaskRemainingUses(MaskType mask)
        {
            if (maskUseCount.TryGetValue(mask, out int count))
            {
                return Mathf.Max(0, maxMaskUses - count);
            }
            return maxMaskUses;
        }

        /// <summary>
        /// Check if a mask is depleted (destroyed)
        /// </summary>
        public bool IsMaskDepleted(MaskType mask)
        {
            return !CanUseMask(mask);
        }

        /// <summary>
        /// Skip current dialogue (for testing)
        /// </summary>
        public void SkipDialogue()
        {
            SubtitleUI.Instance?.SkipTypewriter();
            VoiceManager.Instance?.Stop();
        }

        /// <summary>
        /// Restart the game
        /// </summary>
        public void RestartGame()
        {
            StopAllCoroutines();
            TensionMeter.Instance?.ResetTension();
            SubtitleUI.Instance?.HideImmediate();
            VoiceManager.Instance?.Stop();
            StartGame();
        }
    }
}
