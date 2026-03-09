using UnityEngine;
using UnityEngine.UI;

public class UIBruteForceFix : MonoBehaviour
{
    private void OnEnable()
    {
        Debug.Log("<color=magenta>[UI Brute Force]</color> Controllo e forzatura permessi UI in corso...");

        // 1. Troviamo il Canvas genitore e assicuriamoci che abbia il Graphic Raycaster
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                Debug.LogWarning("<color=magenta>[UI Brute Force]</color> MANCAVA IL GRAPHIC RAYCASTER sul Canvas! L'ho aggiunto io.");
                raycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
            }
            raycaster.enabled = true;
        }
        else
        {
            Debug.LogError("<color=red>[UI Brute Force]</color> Questo oggetto non è dentro un Canvas! La UI non funzionerà mai.");
        }

        // 2. Troviamo TUTTI i CanvasGroup e li forziamo ad essere interagibili
        CanvasGroup[] allGroups = GetComponentsInParent<CanvasGroup>();
        foreach (var cg in allGroups)
        {
            if (!cg.blocksRaycasts || !cg.interactable)
            {
                Debug.LogWarning($"<color=magenta>[UI Brute Force]</color> Il CanvasGroup su {cg.gameObject.name} aveva i blocchi disattivati. Forzatura attivata.");
                cg.blocksRaycasts = true;
                cg.interactable = true;
            }
        }

        // 3. Forziamo Raycast Target sui bottoni principali (cerca i Button)
        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (var btn in buttons)
        {
            Image btnImage = btn.GetComponent<Image>();
            if (btnImage != null && !btnImage.raycastTarget)
            {
                Debug.LogWarning($"<color=magenta>[UI Brute Force]</color> Il bottone {btn.gameObject.name} non aveva Raycast Target attivo. Risolto.");
                btnImage.raycastTarget = true;
            }
        }
    }
}