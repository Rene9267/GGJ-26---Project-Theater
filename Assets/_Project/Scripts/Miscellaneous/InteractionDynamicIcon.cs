using UnityEngine;
using UnityEngine.UI;

public class InteractionDynamicIcon : DirectionIcon
{
    public Image InteractionIcon;

    void Awake()
    {
        SetActiveInteractionIcon(false);
    }

    public void SetActiveInteractionIcon(bool isActive)
    {
        InteractionIcon.gameObject.SetActive(isActive);
        ChangableImage.gameObject.SetActive(!isActive);
    }

    public void SetInteractionIconSprite(Sprite newSprite)
    {
        if(newSprite == null)
        {
            DevLog.LogWarning($"[{this.gameObject}]: Attenzione non hai assegnato la nuova sprite per il bottone di interazione");
            return;
        }
        InteractionIcon.sprite = newSprite;
    }
}
