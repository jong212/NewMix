using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldIngredientItemUI : MonoBehaviour
{
    public Image iconImage;

    public void SetIcon(Sprite icon)
    {
        iconImage.sprite = icon;
    }
}
