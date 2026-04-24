using UnityEngine;

public class Interactable : MonoBehaviour
{
    // Outline _outline; // Requires QuickOutline package - disabled

    void Start()
    {
        // _outline = GetComponent<Outline>(); // Requires QuickOutline package
        // _outline.enabled = false;
    }

    public void EnableOutline()
    {
        // _outline.enabled = true; // Requires QuickOutline package
    }

    public void DisableOutline()
    {
        // _outline.enabled = false; // Requires QuickOutline package
    }
}
