using UnityEngine;
using System.Collections.Generic;

public class WordLinkedElement : MonoBehaviour
{
    [Header("Link dati")]
    public int areaID;
    public string wordID;
    public bool startInactive = false;

    [Header("Debug (read-only)")]
    [SerializeField] private List<string> activeEffects = new List<string>();

    private Collider2D objectCollider;
    private Rigidbody2D objectRigidbody;
    private SpriteRenderer objectRenderer;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private float originalMass;
    private Color originalColor;

    private float linearDampingMovable = 5f;
    private float angularDampingMovable = 2f;

    void Awake()
    {
        objectCollider = GetComponent<Collider2D>();
        objectRigidbody = GetComponent<Rigidbody2D>();
        objectRenderer = GetComponent<SpriteRenderer>();

        if (objectCollider == null)
            Debug.LogError($"Collider2D mancante su {name}");
        if (objectRigidbody == null)
            Debug.LogError($"Rigidbody2D mancante su {name}");
        if (objectRenderer == null)
            Debug.LogError($"SpriteRenderer mancante su {name}");

        originalScale = transform.localScale;
        targetScale = originalScale;

        if (objectRigidbody != null)
        {
            originalMass = objectRigidbody.mass;
            objectRigidbody.bodyType = RigidbodyType2D.Kinematic;
            objectRigidbody.simulated = !startInactive;
        }

        if (objectRenderer != null)
        {
            originalColor = objectRenderer.color;
            objectRenderer.enabled = !startInactive;
        }

        if (objectCollider != null)
            objectCollider.enabled = !startInactive;

        gameObject.layer = LayerMask.NameToLayer("Interactable");
    }

    void Update()
    {
        // Lerping della scala per Bold
        transform.localScale = Vector3.MoveTowards(transform.localScale, targetScale, Time.deltaTime * 1.5f);
    }

    public void OnWordEffectsChanged(List<string> newEffects)
    {
        activeEffects.Clear();
        activeEffects.AddRange(newEffects);

        ApplyEffects();
        DebugActiveEffects();
    }

    private void ApplyEffects()
    {
        bool hasBold = activeEffects.Contains("Bold");
        bool hasItalic = activeEffects.Contains("Italic");
        bool hasStrike = activeEffects.Contains("Strikethrough");
        bool hasHighlight = activeEffects.Contains("Highlight");
        bool hasUnderline = activeEffects.Contains("Underline");

        // --- SCALA E MASSA (Bold) ---
        targetScale = hasBold ? originalScale * 2f : originalScale;

        if (objectRigidbody != null)
        {
            if (hasItalic)
            {
                // Movimento attivo
                objectRigidbody.bodyType = RigidbodyType2D.Dynamic;
                objectRigidbody.simulated = true;
                objectRigidbody.mass = hasBold ? originalMass * 2f : originalMass;
                objectRigidbody.linearDamping = linearDampingMovable;
                objectRigidbody.angularDamping = angularDampingMovable;
            }
            else
            {
                // Movimento disattivo
                objectRigidbody.bodyType = RigidbodyType2D.Kinematic;
                objectRigidbody.simulated = !startInactive;
                objectRigidbody.linearVelocity = Vector2.zero;
                objectRigidbody.angularVelocity = 0f;
                objectRigidbody.mass = originalMass;
            }
        }

        // --- LAYER ---
        if (hasItalic && hasStrike)
            gameObject.layer = LayerMask.NameToLayer("PlayerOnly");
        else
            gameObject.layer = LayerMask.NameToLayer("Interactable");

        // --- COLLIDER ---
        if (hasUnderline)
        {
            objectCollider.enabled = !startInactive; // underline sovrascrive tutto
        }
        else if (hasItalic && hasStrike)
        {
            objectCollider.enabled = true; // solo Player collides via layer
        }
        else if (hasStrike)
        {
            objectCollider.enabled = false; // strike da solo
        }
        else
        {
            objectCollider.enabled = !startInactive; // default
        }

        // --- VISIBILITÀ E COLORE ---
        if (hasUnderline)
        {
            objectRenderer.enabled = true;
            objectRenderer.color = Color.magenta; // underline attivo = viola
        }
        else if (hasHighlight)
        {
            objectRenderer.enabled = true;
            objectRenderer.color = Color.white; // illumina
        }
        else
        {
            objectRenderer.enabled = !startInactive;
            objectRenderer.color = originalColor;
        }
    }

    private void DebugActiveEffects()
    {
        if (activeEffects.Count == 0) return;
        Debug.Log($"[WordLinkedElement] '{name}' effetti attivi: {string.Join(", ", activeEffects)}");
    }
}
