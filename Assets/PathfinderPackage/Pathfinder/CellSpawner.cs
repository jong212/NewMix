using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CellSpawner : MonoBehaviour 
{
	private List<RoomData> rooms;
	
	public static CellSpawner Instance{get{return instance;}}
	private static CellSpawner instance;
	
	void Awake()
	{
		instance = this;
		rooms = new List<RoomData>();
	}
	
	public void DefineCell(int x, int y, RoomData.Type type)
	{
		if(GetRoomAtGridPosition(x, y) == null)
		{
			rooms.Add(new RoomData(new Vector2(x, y), type));
		}
		else
		{
			GetRoomAtGridPosition(x, y).type = type;
		}
	}
	
	void ObliterateWorld()
	{
		rooms = new List<RoomData>();
	}
	
	public RoomData GetRoomAtGridPosition(Vector2 pos)
	{
		return GetRoomAtGridPosition((int)pos.x, (int)pos.y);
	}
	
	public RoomData GetRoomAtGridPosition(int x, int y)
	{
		foreach(RoomData room in rooms)
		{
			if(room.GridPos.x == x && room.GridPos.y == y)
			{
				return room;
			}
		}
		
		return null;
	}
}

public class RoomData
{
	public enum Type
	{
		Passable,
		Impassable
	};
	
	public RoomData(Vector2 gridPos, Type type)
	{
		GridPos = gridPos;
		this.type = type;
	}
	
	bool GetNeighborPossible (Vector2 direction)
	{
		RoomData otherRoom = CellSpawner.Instance.GetRoomAtGridPosition(GridPos + direction);
		return  otherRoom != null && otherRoom.type == RoomData.Type.Passable;
	}
	
	public bool NPossible
	{
		get
		{
			return GetNeighborPossible (new Vector2(0, 1));
		}
	}
	public bool EPossible
	{
		get
		{
			return GetNeighborPossible (new Vector2(1, 0));
		}
	}
	public bool SPossible
	{
		get
		{
			return GetNeighborPossible (new Vector2(0, -1));
		}
	}
	public bool WPossible
	{
		get
		{
			return GetNeighborPossible (new Vector2(-1, 0));
		}
	}
	
	public bool NEPossible
	{
		get
		{
			return GetNeighborPossible (new Vector2(1, 1));
		}
	}
	public bool NWPossible
	{
		get
		{
			return GetNeighborPossible (new Vector2(-1, 1));
		}
	}
	public bool SEPossible
	{
		get
		{
			return GetNeighborPossible (new Vector2(1, -1));
		}
	}
	public bool SWPossible
	{
		get
		{
			return GetNeighborPossible (new Vector2(-1, -1));
		}
	}
	
	public Type type;
	public Vector2 GridPos;
}
