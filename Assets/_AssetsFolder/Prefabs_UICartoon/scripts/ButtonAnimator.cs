using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections;

public class ButtonAnimator : MonoBehaviour, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private Animator buttonAnimator;
    // Use this for initialization
    void Start()
    {
        buttonAnimator = GetComponent<Animator>();
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        buttonAnimator.SetTrigger("Normal");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        buttonAnimator.SetTrigger("Pressed");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        buttonAnimator.SetTrigger("Normal");
    }
}