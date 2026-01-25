using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TextFragment : MonoBehaviour
{
    public int areaID;      // area di riferimento
    public int wordIndex;   // indice della parola mancante
    public CanvasController canvasController;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Colleziona la parola automaticamente dal trigger
        canvasController.CollectWord(areaID, wordIndex);

        Destroy(gameObject);
    }
}
