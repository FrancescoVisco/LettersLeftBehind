using System.Collections.Generic;
using UnityEngine;

public class DiaryArea : MonoBehaviour
{
    [TextArea(5, 10)]
    public string areaText;

    public DiaryController diaryController;

    [Tooltip("Se true, alcune parole saranno mancanti e da collezionare")]
    public bool textIsIncomplete = true;

    [Tooltip("ID univoco area per persistenza frammenti")]
    public string areaID;

    [Tooltip("Indici delle parole mancanti (0-based) da collezionabile")]
    public List<int> missingWordIndices = new List<int>();

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Recover fragments already collected by GameController
        List<int> collected = GameController.Instance != null ? 
                              GameController.Instance.GetCollectedFragmentsForArea(areaID) : 
                              new List<int>();

        diaryController.LoadTextForArea(areaText, textIsIncomplete, collected, missingWordIndices);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        diaryController.ClearAreaText();
    }
}