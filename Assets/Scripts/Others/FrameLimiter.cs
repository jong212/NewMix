using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameLimiter : MonoBehaviour
{
	public enum LimitMode
	{
		PlatformDefault,
		High,
		Medium,
		Low
	}

	[SerializeField] private LimitMode _limit;
	private static LimitMode _Limit;
	public static LimitMode Limit
	{
		get => _Limit;
		set
		{
			_Limit = value;
			SetFrameLimit();
		}
	}

	private void Awake()
	{
		Limit = _limit;
		Destroy(gameObject);
	}

	static void SetFrameLimit()
	{
		QualitySettings.vSyncCount = 0;
		switch (Limit)
		{
			default:
			case LimitMode.PlatformDefault:
				{
					Application.targetFrameRate = -1;
				}
				break;
			case LimitMode.High:
				{
					Application.targetFrameRate = 60;
				}
				break;
			case LimitMode.Medium:
				{
					Application.targetFrameRate = 45;
				}
				break;
			case LimitMode.Low:
				{
					Application.targetFrameRate = 30;
				}
				break;
		}
	}
}
