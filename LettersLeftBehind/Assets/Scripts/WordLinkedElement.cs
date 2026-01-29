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

    [Header("Portali")]
    [SerializeField] private List<WordLinkedElement> linkedPortals = new List<WordLinkedElement>();

    private Collider2D objectCollider;
    private Rigidbody2D objectRigidbody;
    private SpriteRenderer objectRenderer;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private float originalMass;
    private Color originalColor;

    private float linearDampingMovable = 5f;
    private float angularDampingMovable = 2f;

    // Cooldown per RB per portale
    private Dictionary<Rigidbody2D, float> portalCooldowns = new Dictionary<Rigidbody2D, float>();
    private float portalCooldownTime = 0.2f; // piccolo buffer per evitare loop immediati

    void Awake()
    {
        objectCollider = GetComponent<Collider2D>();
        objectRigidbody = GetComponent<Rigidbody2D>();
        objectRenderer = GetComponent<SpriteRenderer>();

        if (objectCollider == null) Debug.LogError($"Collider2D mancante su {name}");
        if (objectRigidbody == null) Debug.LogError($"Rigidbody2D mancante su {name}");
        if (objectRenderer == null) Debug.LogError($"SpriteRenderer mancante su {name}");

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
        transform.localScale = Vector3.MoveTowards(transform.localScale, targetScale, Time.deltaTime * 1.5f);

        // Aggiorna cooldown portali
        List<Rigidbody2D> keys = new List<Rigidbody2D>(portalCooldowns.Keys);
        foreach (var rb in keys)
        {
            portalCooldowns[rb] -= Time.deltaTime;
            if (portalCooldowns[rb] <= 0f)
                portalCooldowns.Remove(rb);
        }
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

        // --- SCALA (Bold) ---
        targetScale = hasBold ? originalScale * 2f : originalScale;

        // --- RIGIDBODY ---
        if (objectRigidbody != null)
        {
            if (hasItalic)
            {
                objectRigidbody.bodyType = RigidbodyType2D.Dynamic;
                objectRigidbody.simulated = true;
                objectRigidbody.mass = hasBold ? originalMass * 2f : originalMass;
                objectRigidbody.linearDamping = linearDampingMovable;
                objectRigidbody.angularDamping = angularDampingMovable;
            }
            else if (hasUnderline)
            {
                // Portale deve essere Dynamic per teletrasporto
                objectRigidbody.bodyType = RigidbodyType2D.Dynamic;
                objectRigidbody.simulated = true;
                objectRigidbody.mass = originalMass;
                objectRigidbody.linearDamping = 0f;
                objectRigidbody.angularDamping = 0f;
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

        // --- LAYER ---
        if (hasItalic && hasStrike)
            gameObject.layer = LayerMask.NameToLayer("PlayerOnly");
        else
            gameObject.layer = LayerMask.NameToLayer("Interactable");

        // --- COLLIDER E TRIGGER ---
        if (hasUnderline)
        {
            objectCollider.isTrigger = true;
            objectCollider.enabled = true;
        }
        else if (hasItalic && hasStrike)
        {
            objectCollider.isTrigger = false;
            objectCollider.enabled = true;
        }
        else if (hasStrike)
        {
            objectCollider.isTrigger = false;
            objectCollider.enabled = false;
        }
        else
        {
            objectCollider.isTrigger = false;
            objectCollider.enabled = !startInactive;
        }

        // --- COLORE ---
        if (hasUnderline)
        {
            objectRenderer.enabled = true;
            objectRenderer.color = Color.magenta;
            UpdatePortals();
        }
        else if (hasHighlight)
        {
            objectRenderer.enabled = true;
            objectRenderer.color = Color.white;
            ClearPortals();
        }
        else
        {
            objectRenderer.enabled = !startInactive;
            objectRenderer.color = originalColor;
            ClearPortals();
        }
    }

    private void UpdatePortals()
    {
        WordLinkedElement[] allElements = FindObjectsOfType<WordLinkedElement>();
        foreach (var elem in allElements)
        {
            if (elem == this) continue;
            if (elem.activeEffects.Contains("Underline") && !linkedPortals.Contains(elem))
            {
                linkedPortals.Add(elem);
                if (!elem.linkedPortals.Contains(this))
                    elem.linkedPortals.Add(this);
            }
        }
    }

    private void ClearPortals()
    {
        foreach (var elem in linkedPortals)
        {
            elem.linkedPortals.Remove(this);
        }
        linkedPortals.Clear();
    }

    private void DebugActiveEffects()
    {
        if (activeEffects.Count == 0) return;
        Debug.Log($"[WordLinkedElement] '{name}' effetti attivi: {string.Join(", ", activeEffects)}");
        if (linkedPortals.Count > 0)
            Debug.Log($"[WordLinkedElement] '{name}' portali collegati: {linkedPortals.Count}");
    }

    // --- TELETRASPORTO CON COOLDOWN PER RB ---
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!activeEffects.Contains("Underline")) return;
        if (linkedPortals.Count == 0) return;

        Rigidbody2D rb = other.attachedRigidbody;
        if (rb == null) return;

        if (portalCooldowns.ContainsKey(rb)) return; // RB è in cooldown → non teletrasportare

        // Seleziona portale casuale collegato
        WordLinkedElement targetPortal = linkedPortals[Random.Range(0, linkedPortals.Count)];
        if (targetPortal == null) return;

        // Teletrasporto
        rb.position = targetPortal.transform.position;
        rb.linearVelocity = Vector2.zero;

        // Imposta cooldown per entrambi i portali
        portalCooldowns[rb] = portalCooldownTime;
        if (!targetPortal.portalCooldowns.ContainsKey(rb))
            targetPortal.portalCooldowns[rb] = portalCooldownTime;
    }
}
