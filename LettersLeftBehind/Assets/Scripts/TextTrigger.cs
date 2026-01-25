using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class TextTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    public int areaID;
    [TextArea(3,10)]
    public string fullText;

    [Tooltip("Index delle parole mancanti nel testo (0-based)")]
    public List<int> missingWordIndices = new List<int>();

    [HideInInspector]
    public List<CanvasController.WordData> words = new List<CanvasController.WordData>();

    public CanvasController canvasController;

    private void Start()
    {
        words.Clear();
        string[] split = fullText.Split(' ');

        for (int i = 0; i < split.Length; i++)
        {
            CanvasController.WordData wd = new CanvasController.WordData();
            wd.plainText = split[i];
            wd.isMissing = missingWordIndices.Contains(i);
            wd.collectedText = "";
            words.Add(wd);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        canvasController.SetTrigger(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        canvasController.ClearTrigger();
    }
}
