using UnityEngine;

public class EnumArrayAttribute : PropertyAttribute
{
	public string[] names;
	public EnumArrayAttribute(System.Type names_enum_type)
	{
		names = System.Enum.GetNames(names_enum_type);
	}
}