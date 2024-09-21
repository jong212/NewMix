using UnityEngine;

public abstract class Interaction : MonoBehaviour
{
    public abstract bool TryInteract(Character interactor);

    protected void LogInteraction()
    {
        Debug.Log($"{gameObject.name} ==> {GetType().Name}");
    }
}
