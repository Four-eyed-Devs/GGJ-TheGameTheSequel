using System;
using UnityEngine;

namespace Interrogation.Dialogue
{
    /// <summary>
    /// Manages the tension meter for the interrogation.
    /// Tension starts at 50/100. Below 50 = low tension (bad path), 50+ = high tension (good path).
    /// Correct mask = -10 tension, Wrong mask = +10 tension.
    /// </summary>
    public class TensionMeter : MonoBehaviour
    {
        public static TensionMeter Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private int startingTension = 50;
        [SerializeField] private int maxTension = 100;
        [SerializeField] private int minTension = 0;
        [SerializeField] private int tensionChangeAmount = 10;

        [Header("Debug")]
        [SerializeField] private int currentTension;

        /// <summary>
        /// Event fired when tension changes. Parameters: (newValue, delta)
        /// </summary>
        public event Action<int, int> OnTensionChanged;

        /// <summary>
        /// Event fired when tension crosses the 50 threshold
        /// </summary>
        public event Action<bool> OnTensionThresholdCrossed; // true = now high, false = now low

        public int CurrentTension => currentTension;
        public bool IsHighTension => currentTension >= 50;
        public bool IsLowTension => currentTension < 50;
        public float TensionNormalized => (float)currentTension / maxTension;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            ResetTension();
        }

        /// <summary>
        /// Reset tension to starting value
        /// </summary>
        public void ResetTension()
        {
            currentTension = startingTension;
            OnTensionChanged?.Invoke(currentTension, 0);
        }

        /// <summary>
        /// Called when player uses the correct mask - decreases tension
        /// </summary>
        public void OnCorrectMask()
        {
            ChangeTension(-tensionChangeAmount);
            Debug.Log($"[TensionMeter] Correct mask used! Tension decreased to {currentTension}");
        }

        /// <summary>
        /// Called when player uses the wrong mask - increases tension
        /// </summary>
        public void OnWrongMask()
        {
            ChangeTension(tensionChangeAmount);
            Debug.Log($"[TensionMeter] Wrong mask used! Tension increased to {currentTension}");
        }

        /// <summary>
        /// Change tension by a specific amount
        /// </summary>
        public void ChangeTension(int delta)
        {
            bool wasHighTension = IsHighTension;
            int previousTension = currentTension;
            
            currentTension = Mathf.Clamp(currentTension + delta, minTension, maxTension);
            
            OnTensionChanged?.Invoke(currentTension, currentTension - previousTension);

            // Check if we crossed the threshold
            bool isNowHighTension = IsHighTension;
            if (wasHighTension != isNowHighTension)
            {
                OnTensionThresholdCrossed?.Invoke(isNowHighTension);
                Debug.Log($"[TensionMeter] Threshold crossed! Now {(isNowHighTension ? "HIGH" : "LOW")} tension");
            }
        }

        /// <summary>
        /// Get the ending type based on current tension
        /// </summary>
        public string GetEndingType()
        {
            // Tension < 50 means player stayed calm = good ending (escape)
            // Tension >= 50 means player got stressed = bad ending (caught)
            return IsLowTension ? "good" : "bad";
        }

        /// <summary>
        /// Set tension to a specific value (for testing/loading)
        /// </summary>
        public void SetTension(int value)
        {
            bool wasHighTension = IsHighTension;
            int previousTension = currentTension;
            
            currentTension = Mathf.Clamp(value, minTension, maxTension);
            
            OnTensionChanged?.Invoke(currentTension, currentTension - previousTension);

            bool isNowHighTension = IsHighTension;
            if (wasHighTension != isNowHighTension)
            {
                OnTensionThresholdCrossed?.Invoke(isNowHighTension);
            }
        }
    }
}
