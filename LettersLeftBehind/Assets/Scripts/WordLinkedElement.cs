using UnityEngine;
using System.Collections.Generic;

public class WordLinkedElement : MonoBehaviour
{
    public int areaID;
    public string wordID;

    public void OnWordEffectsChanged(List<string> effects)
    {
        if (effects.Count == 0)
            return;

        Debug.Log($"Elemento '{name}' ha i seguenti effetti attivi: {string.Join(", ", effects)}");

        if (effects.Contains("Strikethrough")) OnStrikethrough();
        if (effects.Contains("Bold")) OnBold();
        if (effects.Contains("Italic")) OnItalic();
        if (effects.Contains("Highlight")) OnHighlight();
        if (effects.Contains("Underline")) OnUnderline();
    }

    void OnStrikethrough()
    {

    }

    void OnBold()
    {

    }

    void OnItalic()
    {

    }

    void OnHighlight()
    {

    }

    void OnUnderline()
    {

    }
}
