using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Resource Bank")]
public class ResourceBank : ScriptableObject
{
	[SerializeField] private List<ScriptableObject> _items;
	private Dictionary<short, ScriptableObject> _forwardDictionary;
	private Dictionary<short, ScriptableObject> ForwardDictionary
	{
		get
		{
			if (_forwardDictionary == null)
			{
				_forwardDictionary = new Dictionary<short, ScriptableObject>();
				short i = 0;
				foreach (ScriptableObject value in _items)
				{
					_forwardDictionary.Add((short)(i + 1), value);
					i++;
				}
			}
			return _forwardDictionary;
		}
	}

	private Dictionary<ScriptableObject, short> _reverseDictionary = null;
	private Dictionary<ScriptableObject, short> ReverseDictionary
	{
		get
		{
			if (_reverseDictionary == null)
			{
				_reverseDictionary = new Dictionary<ScriptableObject, short>();
				short i = 0;
				foreach (ScriptableObject value in _items)
				{
					_reverseDictionary.Add(value, (short)(i + 1));
					i++;
				}
			}
			return _reverseDictionary;
		}
	}

	public short GetKey(ScriptableObject value)
	{
		return ReverseDictionary[value];
	}

	public ScriptableObject GetValue(short key)
	{
		if (ForwardDictionary.TryGetValue(key, out var value))
			return value;
		return null;
	}

	public T GetValue<T>(short key) where T : ScriptableObject
	{
		if (ForwardDictionary.TryGetValue(key, out var value))
			return (T)value;
		return null;
	}
}