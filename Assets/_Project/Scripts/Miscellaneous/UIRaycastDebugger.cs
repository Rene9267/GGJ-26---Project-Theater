using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UIRaycastDebugger : MonoBehaviour
{
    void Update()
    {
        // Se non c'è un mouse collegato, ignoriamo
        if (Mouse.current == null) return;

        // Quando clicchiamo il tasto sinistro
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current == null)
            {
                Debug.LogWarning("<color=red>[UI Debugger]</color> Nessun EventSystem trovato nella scena! I bottoni non funzioneranno mai senza di esso.");
                return;
            }

            // Prepariamo un "finto pointer" alla posizione attuale del mouse
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Mouse.current.position.ReadValue()
            };

            // Lanciamo il raggio che attraversa tutta la UI
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count > 0)
            {
                Debug.Log($"<color=cyan>[UI Debugger]</color> Click intercettato dal primo oggetto in alto: <b>{results[0].gameObject.name}</b>");

                // Stampiamo anche cosa c'è dietro, per capire la gerarchia
                for (int i = 1; i < results.Count; i++)
                {
                    Debug.Log($"   <i>Dietro c'è: {results[i].gameObject.name}</i>");
                }
            }
            else
            {
                Debug.Log("<color=orange>[UI Debugger]</color> Cliccato nel vuoto. Nessun oggetto UI con 'Raycast Target' attivo è stato colpito.");
            }
        }
    }
}