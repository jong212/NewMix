using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class PathfindConstants
{
	public static readonly float GridSize = 2; //Change this to the width and height of the grid
	
	public enum Directions
	{	
		North,
		East,
		South,
		West,
		NorthEast,
		NorthWest,
		SouthEast,
		SouthWest,
		None
	}
	
	public static Vector2 GridToWorld(int x, int y)
	{
		return RoundedVector(x * GridSize, y * GridSize);
	}
	
	public static Vector2 WorldToGrid(int x, int y)
	{
		return RoundedVector(x / GridSize, y / GridSize);
	}	
	
	public static Vector2 GridToWorld(float x, float y)
	{
		return GridToWorld(Mathf.RoundToInt(x), Mathf.RoundToInt(y));	
	}
	
	public static Vector2 WorldToGrid(float x, float y)
	{
		return WorldToGrid(Mathf.RoundToInt(x), Mathf.RoundToInt(y));	
	}
	
	public static Vector2 WorldToGrid(Vector3 worldPos)
	{
		return WorldToGrid(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.z));
	}
	
	public static Vector2 RoundedVector(float x, float y)
	{
		return new Vector2(Mathf.Round(x), Mathf.Round(y));	
	}
}
