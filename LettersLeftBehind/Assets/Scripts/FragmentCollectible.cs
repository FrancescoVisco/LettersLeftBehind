using UnityEngine;

public class FragmentCollectible : MonoBehaviour
{
    [Tooltip("ID area a cui appartiene")]
    public string areaID;

    [Tooltip("Indice della parola nel DiaryController da sbloccare")]
    public int wordIndex;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Find the DiaryController in the scene
        DiaryController diary = Object.FindFirstObjectByType<DiaryController>();
        if (diary != null)
        {
            diary.CollectFragment(wordIndex); // sblocca la parola
        }

        // Save persistence in GameController
        if (GameController.Instance != null)
        {
            GameController.Instance.MarkFragmentCollected(areaID, wordIndex);
        }

        // Hide or destroy the collectible
        gameObject.SetActive(false);
    }
}