using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TextFragment : MonoBehaviour
{
    public int areaID;
    public int wordIndex;
    public CanvasController canvasController;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        canvasController.CollectWord(areaID, wordIndex);

        Destroy(gameObject);
    }
}
