using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DiaryController : MonoBehaviour
{
    // WORD DATA
    [System.Serializable]
    public class WordData
    {
        public string text;
        public List<string> effects = new List<string>(); // Bold, Italic, Underline, Strike, Highlight
        public bool isCollected = true;         
        public bool isMissingFragment = false;   

        public WordData(string t, bool missing = false)
        {
            text = t;
            isMissingFragment = missing;
            isCollected = !missing;
        }

        public string GetFormattedWord()
        {
            if (!isCollected) return "_____";

            string result = text;

            foreach (var effect in effects)
            {
                switch (effect)
                {
                    case "Bold": result = $"<b>{result}</b>"; break;
                    case "Italic": result = $"<i>{result}</i>"; break;
                    case "Underline": result = $"<u>{result}</u>"; break;
                    case "Strike": result = $"<s>{result}</s>"; break;
                    case "Highlight": result = $"<mark=#FFF59D>{result}</mark>"; break;
                }
            }
            return result;
        }
    }

    // UI REFERENCE
    [Header("UI")]
    public GameObject diaryCanvas;
    public TextMeshProUGUI diaryText;
    public GameObject crosshair; // Riferimento all’immagine del crosshair

    [Header("Player")]
    public MonoBehaviour movementScript;

    [Header("Testo")]
    [TextArea(5, 10)]
    public string baseText;

    // STATE
    private List<WordData> words = new List<WordData>();
    private int selectedWordIndex = -1;
    private bool diaryOpen = false;

    void Start()
    {
        diaryCanvas.SetActive(false);
        if (crosshair != null) crosshair.SetActive(true); // Assicura che il crosshair sia visibile all’inizio
        BuildWords();
        RefreshText();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleDiary();
    }

    // DIARY UI
    void ToggleDiary()
    {
        diaryOpen = !diaryOpen;
        diaryCanvas.SetActive(diaryOpen);

        // Movimenti
        if (movementScript != null)
            movementScript.enabled = !diaryOpen;

        // Crosshair
        if (crosshair != null)
            crosshair.SetActive(!diaryOpen);

        Cursor.lockState = diaryOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = diaryOpen;
    }

    // TEXT COSTRUCTION
    void BuildWords()
    {
        words.Clear();
        string[] split = baseText.Split(' ');

        foreach (string w in split)
            words.Add(new WordData(w));
    }

    // LOAD AREA TEXT WITH MISSING WORDS
    public void LoadTextForArea(string newText, bool isIncomplete, List<int> collectedFragments = null, List<int> missingIndices = null)
    {
        baseText = newText;
        words.Clear();

        string[] split = baseText.Split(' ');

        for (int i = 0; i < split.Length; i++)
        {
            bool missing = false;

            if (isIncomplete && missingIndices != null && missingIndices.Contains(i))
            {
                missing = (collectedFragments == null || !collectedFragments.Contains(i));
            }

            words.Add(new WordData(split[i], missing));
        }

        selectedWordIndex = -1;
        RefreshText();
    }

    // HIDE AREA TEXT
    public void ClearAreaText()
    {
        words.Clear();
        selectedWordIndex = -1;
        diaryText.text = "";
    }

    // REFRESH DIARY TEXT
    void RefreshText()
    {
        diaryText.text = "";

        for (int i = 0; i < words.Count; i++)
        {
            string word = words[i].GetFormattedWord();
            word = $"<link={i}>{word}</link>";

            if (i == selectedWordIndex)
                word = $"<color=#88AAFF>{word}</color>";

            diaryText.text += word + " ";
        }
    }

    // CLICK WORD
    public void OnPointerClick(BaseEventData data)
    {
        if (!diaryOpen) return;

        PointerEventData pointerData = data as PointerEventData;
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(diaryText, pointerData.position, null);

        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = diaryText.textInfo.linkInfo[linkIndex];
            selectedWordIndex = int.Parse(linkInfo.GetLinkID());
            RefreshText();
        }
    }

    // WORD EFFECTS
    public void Bold() { ToggleEffect("Bold"); }
    public void Italic() { ToggleEffect("Italic"); }
    public void Underline() { ToggleEffect("Underline"); }
    public void Strike() { ToggleEffect("Strike"); }
    public void Highlight() { ToggleEffect("Highlight"); }

    void ToggleEffect(string effect)
    {
        if (!diaryOpen || selectedWordIndex < 0) return;

        WordData word = words[selectedWordIndex];

        if (word.effects.Contains(effect))
            word.effects.Remove(effect);
        else
        {
            if (word.effects.Count >= 2) return; // max 2 effetti
            word.effects.Add(effect);
        }

        RefreshText();
    }

    // RESET
    public void ResetText()
    {
        foreach (var word in words)
            word.effects.Clear();

        selectedWordIndex = -1;
        RefreshText();
    }

    // COLLECT FRAGMENT
    public void CollectFragment(int wordIndex)
    {
        if (wordIndex < 0 || wordIndex >= words.Count) return;

        WordData word = words[wordIndex];
        if (word.isMissingFragment && !word.isCollected)
        {
            word.isCollected = true;
            RefreshText();
        }
    }
}