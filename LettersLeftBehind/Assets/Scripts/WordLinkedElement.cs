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

        UpdateLayer();      // Gestione layer prima di tutto
        ApplyEffects();     // Applica Bold, Italic, Strike, Highlight
        DebugActiveEffects();
    }

    private void UpdateLayer()
    {
        bool hasItalic = activeEffects.Contains("Italic");
        bool hasStrike = activeEffects.Contains("Strikethrough");

        if (hasItalic && hasStrike)
            gameObject.layer = LayerMask.NameToLayer("PlayerOnly");
        else
            gameObject.layer = LayerMask.NameToLayer("Interactable");
    }

    private void ApplyEffects()
    {
        bool hasBold = activeEffects.Contains("Bold");
        bool hasItalic = activeEffects.Contains("Italic");
        bool hasStrike = activeEffects.Contains("Strikethrough");
        bool hasHighlight = activeEffects.Contains("Highlight");

        // --- SCALA E MASSA (Bold) ---
        targetScale = hasBold ? originalScale * 2f : originalScale;

        if (objectRigidbody != null)
        {
            // Italic attiva movimento
            if (hasItalic)
            {
                objectRigidbody.bodyType = RigidbodyType2D.Dynamic;
                objectRigidbody.simulated = true;
                objectRigidbody.mass = hasBold ? originalMass * 2f : originalMass;
                objectRigidbody.linearDamping = linearDampingMovable;
                objectRigidbody.angularDamping = angularDampingMovable;
            }
            else
            {
                objectRigidbody.bodyType = RigidbodyType2D.Kinematic;
                objectRigidbody.simulated = !startInactive;
                objectRigidbody.linearVelocity = Vector2.zero;
                objectRigidbody.angularVelocity = 0f;
                objectRigidbody.mass = originalMass;
            }
        }

        // --- COLLIDER ---
        if (hasItalic && hasStrike)
        {
            // Collide solo con Player, sempre attivo
            objectCollider.enabled = true;
        }
        else if (hasStrike)
        {
            objectCollider.enabled = false;
        }
        else
        {
            objectCollider.enabled = !startInactive;
        }

        // --- VISIBILITÀ E ILLUMINAZIONE (Highlight) ---
        if (hasHighlight)
        {
            if (objectRenderer != null)
            {
                objectRenderer.enabled = true;
                objectRenderer.color = Color.white; // illumina
            }

            if (!hasStrike)
                objectCollider.enabled = true; // collider attivo se Strike non presente
        }
        else
        {
            SetVisibility(!startInactive);
            if (objectRenderer != null)
                objectRenderer.color = originalColor;
        }
    }

    private void SetVisibility(bool visible)
    {
        if (objectRenderer != null)
            objectRenderer.enabled = visible;

        if (objectCollider != null)
            objectCollider.enabled = visible;
    }

    private void DebugActiveEffects()
    {
        if (activeEffects.Count == 0) return;
        Debug.Log($"[WordLinkedElement] '{name}' effetti attivi: {string.Join(", ", activeEffects)}");
    }
}
