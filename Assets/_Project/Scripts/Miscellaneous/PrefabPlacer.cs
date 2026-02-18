#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PrefabPlacer : ScriptableWizard
{
    #region Fields
    [Tooltip("Il prefab che vuoi piazzare")]
    public GameObject PlaceablePrefab;
    [Tooltip("Gli oggetti che vuoi sostituire con il prefab")]
    public List<GameObject> ReplaceableItems;
    [Tooltip("Se vuoi mantenere la gerarchia degli oggetti sostituiti, spunta questa opzione")]
    public bool KeepHierarchy = true;
    [Tooltip("Se vuoi distruggere gli oggetti sostituiti dopo aver piazzato il prefab, spunta questa opzione")]
    public bool DestroyAfterReplace = false;

    #endregion


    #region Class Methods

    [MenuItem("Tools/Rene's Miscellaneous/Prefab Replacer")]
    static void CreateWizard()
    {
        DisplayWizard<PrefabPlacer>("Piazza Prefab sugli Oggetti", "Esegui");
    }

    void OnWizardCreate()
    {
        if (PlaceablePrefab == null)
        {
            DevLog.LogError("PrefabPlacer: Nessun prefab assegnato. Per favore assegna un prefab da piazzare sugli oggetti sostituibili.");
            return;
        }

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Sostituzione Oggetti");
        var undoGroupIndex = Undo.GetCurrentGroup();

        foreach (var oldObject in ReplaceableItems)
        {
            if (oldObject == null) continue;

            GameObject nuovoOggetto = (GameObject)PrefabUtility.InstantiatePrefab(PlaceablePrefab);

            Undo.RegisterCreatedObjectUndo(nuovoOggetto, "Nuovo Oggetto");

            nuovoOggetto.transform.SetPositionAndRotation(oldObject.transform.position, oldObject.transform.rotation);
            nuovoOggetto.transform.localScale = oldObject.transform.localScale;

            if (KeepHierarchy)
            {
                nuovoOggetto.transform.SetParent(oldObject.transform.parent);
                nuovoOggetto.transform.SetSiblingIndex(oldObject.transform.GetSiblingIndex());
            }

            Undo.DestroyObjectImmediate(oldObject);
        }

        Undo.CollapseUndoOperations(undoGroupIndex);
    }

    #endregion
}

#endif
