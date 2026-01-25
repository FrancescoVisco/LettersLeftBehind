using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class CanvasController : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI textUI;
    [Header("Selection")]
    public Color selectedWordColor = Color.cyan;
    private List<WordData> words = new List<WordData>();
    private int selectedWordIndex = -1;
    private bool hasActiveText = false;
    private TextTrigger currentTrigger = null;

    [System.Serializable]
    public class WordData
    {
        public string plainText;
        public List<string> effects = new List<string>();
        public bool isMissing = false;
        public string collectedText = "";
        public int wordIndex;

        public string GetDisplayedText()
        {
            if (isMissing && string.IsNullOrEmpty(collectedText))
                return "-----";
            return string.IsNullOrEmpty(collectedText) ? plainText : collectedText;
        }

        public string GetFormattedText(bool selected, string selectionColorTag)
        {
            string text = GetDisplayedText();

            foreach (string effect in effects)
            {
                switch (effect)
                {
                    case "Bold": text = $"<b>{text}</b>"; break;
                    case "Italic": text = $"<i>{text}</i>"; break;
                    case "Underline": text = $"<u>{text}</u>"; break;
                    case "Strikethrough": text = $"<s>{text}</s>"; break;
                    case "Highlight": text = $"<mark=#FFFF0080>{text}</mark>"; break;
                }
            }

            if (selected)
                text = $"<color={selectionColorTag}>{text}</color>";

            return text;
        }
    }

    void Update()
    {
        if (hasActiveText)
            DetectWordClick();
    }

    void DetectWordClick()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        int wordIndex = TMP_TextUtilities.FindIntersectingWord(textUI, Input.mousePosition, null);

        if (wordIndex != -1)
        {
            WordData wd = words[wordIndex];
            if (wd.isMissing && string.IsNullOrEmpty(wd.collectedText))
                return;

            selectedWordIndex = wordIndex;
            RefreshText();
        }
    }

    public void SetTrigger(TextTrigger trigger)
    {
        currentTrigger = trigger;
        words = trigger.words;
        for (int i = 0; i < words.Count; i++)
            words[i].wordIndex = i;
        selectedWordIndex = -1;
        hasActiveText = true;
        RefreshText();
    }

    public void ClearTrigger()
    {
        selectedWordIndex = -1;
        hasActiveText = false;
        textUI.text = "";
        currentTrigger = null;
    }

    public void CollectWord(int areaID, int wordIndex)
    {
        TextTrigger trigger = null;

        if (currentTrigger != null && currentTrigger.areaID == areaID)
            trigger = currentTrigger;
        else
        {
            TextTrigger[] allTriggers = UnityEngine.Object.FindObjectsByType<TextTrigger>(FindObjectsSortMode.None);
            foreach (var t in allTriggers)
            {
                if (t.areaID == areaID)
                {
                    trigger = t;
                    break;
                }
            }
        }

        if (trigger == null) return;
        if (wordIndex < 0 || wordIndex >= trigger.words.Count) return;

        WordData wd = trigger.words[wordIndex];

        if (wd.isMissing && string.IsNullOrEmpty(wd.collectedText))
        {
            wd.collectedText = wd.plainText;
            if (currentTrigger == trigger)
                RefreshText();
        }
    }

    public void ApplyEffect(string effectName)
    {
        if (!hasActiveText || selectedWordIndex < 0) return;

        WordData word = words[selectedWordIndex];
        if (word.isMissing && string.IsNullOrEmpty(word.collectedText)) return;

        if (word.effects.Contains(effectName))
            word.effects.Remove(effectName);
        else if (word.effects.Count < 2)
            word.effects.Add(effectName);

        RefreshText();
        UpdateLinkedElements(word, selectedWordIndex);
    }

    void UpdateLinkedElements(WordData word, int index)
    {
        WordLinkedElement[] elements = UnityEngine.Object.FindObjectsByType<WordLinkedElement>(FindObjectsSortMode.None);
        foreach (var elem in elements)
        {
            if (currentTrigger != null &&
                elem.areaID == currentTrigger.areaID &&
                elem.wordID == index.ToString())
            {
                elem.OnWordEffectsChanged(word.effects);
            }
        }
    }

    public void ResetText()
    {
        foreach (var word in words)
            word.effects.Clear();

        selectedWordIndex = -1;
        RefreshText();
    }

    void RefreshText()
    {
        string colorTag = $"#{ColorUtility.ToHtmlStringRGB(selectedWordColor)}";
        string finalText = "";

        for (int i = 0; i < words.Count; i++)
        {
            bool selected = i == selectedWordIndex;
            finalText += words[i].GetFormattedText(selected, colorTag) + " ";
        }

        textUI.text = finalText.TrimEnd();
    }
}
