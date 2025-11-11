using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public InteractionTag InteractionTag;

    public abstract void Action();
}
