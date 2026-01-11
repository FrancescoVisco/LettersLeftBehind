using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance;


    private Dictionary<string, List<int>> areaFragmentsCollected = new Dictionary<string, List<int>>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    // Returns the fragments already collected for an area
    public List<int> GetCollectedFragmentsForArea(string areaID)
    {
        if (areaFragmentsCollected.ContainsKey(areaID))
            return new List<int>(areaFragmentsCollected[areaID]);

        return new List<int>(); // nessun frammento raccolto
    }

    // Mark a fragment as collected
    public void MarkFragmentCollected(string areaID, int wordIndex)
    {
        if (!areaFragmentsCollected.ContainsKey(areaID))
            areaFragmentsCollected[areaID] = new List<int>();

        if (!areaFragmentsCollected[areaID].Contains(wordIndex))
            areaFragmentsCollected[areaID].Add(wordIndex);
    }
}