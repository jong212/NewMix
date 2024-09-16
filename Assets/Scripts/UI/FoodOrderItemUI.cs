using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FoodOrderItemUI : MonoBehaviour
{
    public Image iconImage;
    public Image barFill;
	public TMP_Text orderNumber;
    public RectTransform rectTransform;
    public GameObject visual;
    public ShakeBehaviour shake;

	public Gradient barGradient;
	public Image overlay;
	public Color defaultColour, errorColour, completeColour;
	public Animator overlayAnimator;

    [Header("----- Ingredients -----")]
    public Transform ingredientHolder;
    public List<FoodOrderIngredientUI> ingredientsUI = new List<FoodOrderIngredientUI>();

	bool expired = false;

	private void OnDisable()
	{
		if (expired) Destroy(gameObject);
	}

	void FlashOverlay(Color col)
	{
		overlay.color = col;
		overlayAnimator.SetTrigger("Blink");
	}

	public void FlashComplete()
	{
        FlashOverlay(completeColour);
    }

    public void FlashError()
	{
		FlashOverlay(errorColour);
	}

	public void Paint(float value)
	{
		if (value < 0) value = 0;

		barFill.fillAmount = value;
		barFill.color = barGradient.Evaluate(barFill.fillAmount);
		if (barFill.fillAmount < 0.1f)
		{
			shake.TriggerShake(1, 0.5f);
		}

		//if (barFill.fillAmount > 0)
		//{
		//	barFill.fillAmount -= 1.0f / maxWaitTime * Time.deltaTime;
		//	barFill.color = barGradient.Evaluate(barFill.fillAmount);
		//	if (barFill.fillAmount < 0.1f)
		//	{
		//		shake.TriggerShake(1, 0.5f);
		//	}
		//}
	}

	public void SetOrder(IngredientData[] ingredients)
	{
		foreach (var ingrData in ingredients)
		{
			CreateIngredientUI(ingrData.Icon);
		}
		SetRectTransform();

		if (AssemblyMap.GetIcon(ingredients.ToHashSet(), out Sprite icon))
		{
			SetIcon(icon);
		}
		else
		{
			Debug.LogError($"What even is {string.Join<IngredientData>(", ", ingredients)}");
		}
	}

    public void SetIcon(Sprite sprite)
    {
        iconImage.sprite = sprite;
    }

    public FoodOrderIngredientUI CreateIngredientUI(Sprite ingredientIcon)
    {
        FoodOrderIngredientUI newIngredientItem = Instantiate(ResourcesManager.instance.ingredientUIPrefab, ingredientHolder);
        newIngredientItem.iconImage.sprite = ingredientIcon;
        ingredientsUI.Add(newIngredientItem);
        return newIngredientItem;
    }

    public void SetRectTransform()
    {
        rectTransform.sizeDelta = new Vector2(100+(ingredientsUI.Count * 50), 140);       
    }

	public void Expire()
	{
		expired = true;
		StartCoroutine(ExpireRoutine());
		IEnumerator ExpireRoutine()
		{
			yield return new WaitForSeconds(0.5f);
			Destroy(gameObject);
		}
	}
}
