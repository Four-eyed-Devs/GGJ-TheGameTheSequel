using System;
using System.Collections.Generic;
using UnityEngine;

namespace Interrogation.Dialogue
{
    /// <summary>
    /// Enum representing the four psychological masks
    /// </summary>
    public enum MaskType
    {
        Logic,      // The Stoic - calm, rational responses
        Emotion,    // The Victim - emotional, sympathetic responses
        Aggression, // The Hothead - aggressive, deflecting responses
        Charm       // The Charmer - charismatic, disarming responses
    }

    /// <summary>
    /// A single investigator reaction line with optional voice clip
    /// </summary>
    [Serializable]
    public class InvestigatorLine
    {
        public string text;
        public string voiceClipPath; // Path to AudioClip in Resources
    }

    /// <summary>
    /// Reactions for a mask answer - 3 for low tension, 3 for high tension
    /// </summary>
    [Serializable]
    public class MaskReactions
    {
        public InvestigatorLine[] lowTension;  // 3 variants for tension < 50
        public InvestigatorLine[] highTension; // 3 variants for tension >= 50
    }

    /// <summary>
    /// A single mask's answer data for a question
    /// </summary>
    [Serializable]
    public class MaskAnswer
    {
        public string maskType;        // "Logic", "Emotion", "Aggression", "Charm"
        public bool isCorrect;         // True if this is the correct mask for this question
        public string playerLine;      // What the player says when using this mask
        public MaskReactions reactions;
    }

    /// <summary>
    /// Complete data for a single question
    /// </summary>
    [Serializable]
    public class QuestionData
    {
        public int questionId;
        public string correctMask;     // Which mask is correct for this question
        public InvestigatorLine investigatorQuestion;
        public MaskAnswer[] maskAnswers; // 4 answers, one per mask type
    }

    /// <summary>
    /// Intro question data with 3 random variants
    /// </summary>
    [Serializable]
    public class IntroData
    {
        public InvestigatorLine[] introVariants; // 3 variants, pick random
    }

    /// <summary>
    /// Game ending data
    /// </summary>
    [Serializable]
    public class EndingData
    {
        public string endingType;      // "good" or "bad"
        public InvestigatorLine[] lines; // Multiple lines for the ending
    }

    /// <summary>
    /// Container for all endings
    /// </summary>
    [Serializable]
    public class EndingsData
    {
        public EndingData goodEnding;
        public EndingData badEnding;
    }

    /// <summary>
    /// Wrapper classes for JSON deserialization
    /// </summary>
    [Serializable]
    public class QuestionDataWrapper
    {
        public QuestionData question;
    }

    /// <summary>
    /// Utility class for loading dialogue data from JSON
    /// </summary>
    public static class DialogueLoader
    {
        private const string DIALOGUE_PATH = "Dialogue/";
        private const string QUESTIONS_PATH = "Dialogue/Questions/";

        /// <summary>
        /// Load intro data from Resources/Dialogue/intro.json
        /// </summary>
        public static IntroData LoadIntro()
        {
            TextAsset json = Resources.Load<TextAsset>(DIALOGUE_PATH + "intro");
            if (json == null)
            {
                Debug.LogError("Failed to load intro.json from Resources/Dialogue/");
                return null;
            }
            return JsonUtility.FromJson<IntroData>(json.text);
        }

        /// <summary>
        /// Load a specific question from Resources/Dialogue/Questions/q{number}.json
        /// </summary>
        public static QuestionData LoadQuestion(int questionNumber)
        {
            string fileName = $"q{questionNumber}";
            TextAsset json = Resources.Load<TextAsset>(QUESTIONS_PATH + fileName);
            if (json == null)
            {
                Debug.LogError($"Failed to load {fileName}.json from Resources/Dialogue/Questions/");
                return null;
            }
            var wrapper = JsonUtility.FromJson<QuestionDataWrapper>(json.text);
            return wrapper?.question;
        }

        /// <summary>
        /// Load endings data from Resources/Dialogue/endings.json
        /// </summary>
        public static EndingsData LoadEndings()
        {
            TextAsset json = Resources.Load<TextAsset>(DIALOGUE_PATH + "endings");
            if (json == null)
            {
                Debug.LogError("Failed to load endings.json from Resources/Dialogue/");
                return null;
            }
            return JsonUtility.FromJson<EndingsData>(json.text);
        }

        /// <summary>
        /// Load voice clip from Resources path
        /// </summary>
        public static AudioClip LoadVoiceClip(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            return Resources.Load<AudioClip>(path);
        }

        /// <summary>
        /// Convert string to MaskType enum
        /// </summary>
        public static MaskType ParseMaskType(string maskName)
        {
            return maskName.ToLower() switch
            {
                "logic" => MaskType.Logic,
                "emotion" => MaskType.Emotion,
                "aggression" => MaskType.Aggression,
                "charm" => MaskType.Charm,
                _ => throw new ArgumentException($"Unknown mask type: {maskName}")
            };
        }
    }
}
