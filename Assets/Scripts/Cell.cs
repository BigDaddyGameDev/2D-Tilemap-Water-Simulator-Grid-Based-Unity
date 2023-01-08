using System;
using System.Numerics;
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

	public void CellUpdate(Tilemap LiquidTilemap, RuleTile WaterTile, RuleTile BlockTile) {

		// Set background color based on cell type
		if (Type == CellType.Solid)
		{
			LiquidTilemap.SetTile(new Vector3Int(X, Y), BlockTile);
			Matrix4x4 newSolidMatrix = Matrix4x4.Scale(new Vector3(1, 1, 1));
			LiquidTilemap.SetTransformMatrix(new Vector3Int(X, Y), newSolidMatrix);
			return;
		}
		else if (Liquid <= 0)
        {
            LiquidTilemap.SetTile(new Vector3Int(X, Y), null);
            return;
		}

		LiquidTilemap.SetTile(new Vector3Int(X, Y), WaterTile);
		
        // Set color based on pressure in cell
        Vector3Int pos = new Vector3Int(X, Y);
        LiquidTilemap.SetTileFlags(pos, TileFlags.None);
        LiquidTilemap.SetColor(pos, Color.Lerp(Color, DarkColor, Liquid / 4f));

        // Fill out cell if cell above it has liquid
        Matrix4x4 newMatrix = Matrix4x4.Scale(new Vector3(1, 1, 1));
        if (Type == CellType.Blank && Top != null && (Top.Liquid > 0.0001f || Top.Bitmask == 4))
        {
            LiquidTilemap.SetTransformMatrix(pos, newMatrix);
        }
        else
        {
            // Set size of Liquid sprite based on liquid value
            newMatrix = Matrix4x4.Scale(new Vector3(1, Mathf.Min(1, Liquid), 1));
            LiquidTilemap.SetTransformMatrix(pos, newMatrix);
        }
    }
}
