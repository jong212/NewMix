using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class HoverOverSelectable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public float scaleModifier = 0.1f;
    private Vector3 defaultScale;

	[HideInInspector]public bool isHovered = false;
    private Selectable selectable;

    private void Awake()
    {
        SetDefaultScale();
        selectable = GetComponent<Selectable>();
    }

    public void SetDefaultScale()
    {
        defaultScale = transform.localScale;
    }
    public void GetDefaultScale()
    {
        transform.localScale = defaultScale;
    }
    public void SetHoveredScale()
    {
       GetHoveredScale();
    }

    public Vector3 GetHoveredScale()
    {
       return transform.localScale = defaultScale + new Vector3(scaleModifier, scaleModifier, scaleModifier);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        transform.localScale = defaultScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (selectable.IsInteractable())
        {
            isHovered = true;
            transform.localScale = GetHoveredScale();
        }
        if (TryGetComponent(out ActionOnSelect aos))
        {
            if (selectable.IsInteractable())
            {
                aos.OnSelect(null);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        transform.localScale = defaultScale;
        if (TryGetComponent(out ActionOnSelect aos))
        {
            aos.OnDeselect(null);
        }
    }

    private void OnDisable()
    {
        isHovered = false;
    }
}
