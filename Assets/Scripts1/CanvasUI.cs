using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasUI : MonoBehaviour
{
    // Start is called before the first frame update
    private void Awake()
    {
        // Check if another instance of this object already exists
        GameObject[] objs = GameObject.FindGameObjectsWithTag("PersistentUI");

        if (objs.Length > 1)
        {
            // If another one exists, destroy this duplicate
            Destroy(this.gameObject);
        }
        else
        {
            // If this is the only one, make it persistent
            DontDestroyOnLoad(this.gameObject);

            // OPTIONAL: Ensure the tag is set correctly for the check above to work
            if (!gameObject.CompareTag("PersistentUI"))
            {
                Debug.LogWarning("PersistentUI script is attached to a GameObject that is not tagged 'PersistentUI'. The persistence check may fail.");
            }
        }
    }
}
