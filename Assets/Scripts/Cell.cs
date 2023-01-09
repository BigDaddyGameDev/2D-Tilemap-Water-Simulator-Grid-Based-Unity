using System;
using System.Numerics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Vector3 = UnityEngine.Vector3;

public enum CellType {
	Blank = 0,
	Solid = 1
}

public enum FlowDirection { 
	Top = 0, 
	Right = 1,
	Bottom = 2, 
	Left = 3
}

public class Cell {

	// Grid index reference
	public int X { get ; private set; }
	public int Y { get; private set; }

	// Amount of liquid in this cell
	public float Liquid { get; set; }

	// Determines if Cell liquid is settled
	private bool _settled;
	public bool Settled { 
		get { return _settled; } 
		set {
			_settled = value; 
			if (!_settled) {
				SettleCount = 0;
			}
		}
	}
	public int SettleCount { get; set; }

	public CellType Type { get; private set; }

	// Neighboring cells
	public Cell Top;
	public Cell Bottom { get; set; }
	public Cell Left { get; set; }
	public Cell Right { get; set; }

	// Shows flow direction of cell
	public int Bitmask { get; set; }
	public bool[] FlowDirections = new bool[4];

	// Liquid colors
	Color Color = Color.cyan;
	Color DarkColor = new Color (0, 0.1f, 0.2f, 1);
	bool ShowFlow;
	bool RenderDownFlowingLiquid;
	bool RenderFloatingLiquid;

	public void Set(int x, int y)
	{
		X = x;
		Y = y;
	}
		
	public void SetType(CellType type) {
		Type = type;
		if (Type == CellType.Solid)
		{
			Liquid = 0;
		}
		UnsettleNeighbors ();
	}

	public void AddLiquid(float amount) {
		Liquid += amount;
		Settled = false;
	}

	public void ResetFlowDirections() {
		FlowDirections [0] = false;
		FlowDirections [1] = false;
		FlowDirections [2] = false;
		FlowDirections [3] = false;
	}

	// Force neighbors to simulate on next iteration
	public void UnsettleNeighbors() {
		if (Top != null)
			Top.Settled = false;
		if (Bottom != null)
			Bottom.Settled = false;
		if (Left != null)
			Left.Settled = false;
		if (Right != null)
			Right.Settled = false;
	}

	public Tile CellUpdate(Tilemap LiquidTilemap, Tile[] waterTiles, Tile BlockTile) {

		// Set background color based on cell type
		if (Type == CellType.Solid)
		{
			return BlockTile;
		}
		else if (Liquid <= 0.001)
        {
            return null;
		}

		// Set Color
        LiquidTilemap.SetTileFlags(new Vector3Int(X, Y), TileFlags.None);
        LiquidTilemap.SetColor(new Vector3Int(X, Y), Color.Lerp(Color, DarkColor, Liquid / 4f));

		// Set full sprite if there's a tile above or liquid > 1
		if (Liquid >= 1 || Type == CellType.Blank && Top != null && (Top.Liquid > 0.0001f || Top.Bitmask == 4))
		{
			return waterTiles[7];
		}
		else
		{
			// Set Sprite proportional to the liquid amount
			float filledAmount = math.remap(0f, 1f, 0f, 7f, Liquid);

			if (filledAmount < 1f)
				return waterTiles[0];
			else if (filledAmount < 2f)
				return waterTiles[1];
			else if (filledAmount < 3f)
				return waterTiles[2];
			else if (filledAmount < 4f)
				return waterTiles[3];
			else if (filledAmount < 5f)
				return waterTiles[4];
			else if (filledAmount < 6f)
				return waterTiles[5];
			else if (filledAmount < 7f)
				return waterTiles[6];
			else
				return waterTiles[7];
		}
    }
}
