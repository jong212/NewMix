using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Tile.
/// This is a fancy object to represent a tile in the example scene. You dont actually need this in your own project for the pathfinder to work.
/// </summary>

[ExecuteInEditMode]
public class Tile : MonoBehaviour 
{
	private static int mouseActive;
	
	public Vector2 gridPos;
	[SerializeField] private bool alignPos;
	
	[SerializeField] private Material ActiveMat;
	[SerializeField] private Material InactiveMat;
	private bool isActive = true;
	private Renderer topRenderer;
	
	private bool longPath;
	
	void Start () 
	{
		if(Application.isPlaying)
		{
			gridPos = PathfindConstants.WorldToGrid(transform.position);
			CellSpawner.Instance.DefineCell((int)gridPos.x, (int)gridPos.y, RoomData.Type.Passable);
			alignPos = true;
			topRenderer = transform.Find("Top").GetComponent<Renderer>();
		}
	}
	
	public void Blink()
	{
		transform.Find("Top").GetComponent<Renderer>().material.color = Color.blue;	
	}
	
	void Update()
	{
		longPath = Input.GetKey(KeyCode.Space);
		
		if(isActive && Application.isPlaying && topRenderer.material.color != Color.white)
		{
			topRenderer.material.color = Color.Lerp(transform.Find("Top").GetComponent<Renderer>().material.color, Color.white, Time.deltaTime * 5);	
		}
		
		if(alignPos) //if we are in edit mode, align the position when the bool is true. This was for my convenience when setting up the example.
		{
			gridPos = PathfindConstants.WorldToGrid(transform.position);
			
			Vector3 position = PathfindConstants.GridToWorld(gridPos.x, gridPos.y);
			
			transform.position = new Vector3(position.x, 0, position.y);
			alignPos = false;
		}
	}

	public void SetActive (bool isActive) //Set the tile to active or inactive! inactive tiles block the pathfinder.
	{
		this.isActive = isActive;
		CellSpawner.Instance.DefineCell((int)gridPos.x, (int)gridPos.y, isActive? RoomData.Type.Passable : RoomData.Type.Impassable);
		
		topRenderer.material = isActive? ActiveMat : InactiveMat;
	}
	
	void OnMouseOver() //Toggle active! hold down the mouse and drag to make a path!
	{
		if(Input.GetMouseButtonDown(0))
			Tester.Instance.MoveToPos(gridPos, longPath);	
		
		if(Input.GetMouseButton(1) || Input.GetKey(KeyCode.LeftShift))
		{
			if(mouseActive == -1)
				mouseActive = isActive? 0:1;
			
			SetActive (mouseActive == 1? true:false);
		}
		else
		{
			mouseActive = -1;	
		}
	}
}
