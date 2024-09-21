using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarUI : WorldUI
{
    public Image fill;
	[ReadOnly] public Transform target;

	public override void LateUpdate()
	{
		base.LateUpdate();
		if (target)
		{
			transform.position = target.position;
		}
	}

	public void Init(Transform target)
	{
		this.target = target;
		SetFill(0);
	}

	public void SetFill(float amt)
	{
		Show(amt > 0);
		fill.fillAmount = amt;
	}

	public void Show(bool show)
	{
		gameObject.SetActive(show);
	}
}
