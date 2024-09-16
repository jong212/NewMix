using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class WorldNickname : WorldUI
{
	public TMP_Text worldNicknameText;
	public Vector3 offset;

	[HideInInspector] public Transform target;

	public void SetTarget(Transform tgt, string nickname)
	{
        target = tgt; //want to grab interpolation target?		
		worldNicknameText.text = nickname;
	}

	public override void LateUpdate()
	{
		base.LateUpdate();
		if (target)
		{
			transform.position = target.position + offset;
		}
		else
		{
			DestroySelf(1);
		}
	}

	public override void DestroySelf(float length)
	{
        StartCoroutine(WaitAndDestroy(length));
    }

    public override IEnumerator WaitAndDestroy(float length)
	{
		yield return new WaitForSeconds(length);
		if (target != null && !target.Equals(null))
		{
			// continue following the target
			yield return null;
		}
		else // there has been no target so Destroy this:
		{
			Destroy(gameObject);
		}
	}
}
