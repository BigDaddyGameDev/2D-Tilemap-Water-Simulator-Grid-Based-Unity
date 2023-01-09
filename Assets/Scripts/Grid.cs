using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System.Drawing;
using System;
using System.Reflection;
using System.Collections;

public class Grid : MonoBehaviour {

	public int Size = 64;

	public Cell[,] Cells;

	public LiquidSimulator LiquidSimulator;

	public Cell CellPrefab;

	public Tilemap LiquidTilemap;
	public Tile BlockTile;
	public Tile[] WaterTiles;

    bool Fill;

	public float UpdateDelayTime = 0.5f;

    void Awake() {

		// Generate our cells 
		CreateGrid ();

		// Initialize the liquid simulator
		if(LiquidSimulator == null)
			LiquidSimulator = new LiquidSimulator();

		LiquidSimulator.Initialize (Cells);

		StartCoroutine(DelayExecuteSim(UpdateDelayTime));
	}

	void CreateGrid() {

		Cells = new Cell[Size, Size];

		// Cells
		for (int x = 0; x < Size; x++)
		{
			for (int y = 0; y < Size; y++)
			{
				LiquidTilemap.SetTile(new Vector3Int(x,y,0), null);
				
				Cell cell = new Cell();

				cell.Set (x, y);

				// Add border
				if (x == 0 || y == 0 || x == Size - 1 || y == Size - 1)
				{
					LiquidTilemap.SetTile(new Vector3Int(x, y), BlockTile);
					cell.SetType(CellType.Solid);
				}
				
                Cells [x, y] = cell;
			}
		}
		UpdateNeighbors ();
	}

	// Sets neighboring cell references
	void UpdateNeighbors() {
		for (int x = 0; x < Size; x++) {

			for (int y = 0; y < Size; y++) {

				// Left most cells do not have left neighbor
				if (x > 0 )
				{
					Cells[x, y].Left = Cells [x - 1, y];
				}
				// Right most cells do not have right neighbor
				if (x < Size - 1)
				{
					Cells[x, y].Right = Cells [x + 1, y];
				}
                // bottom most cells do not have bottom neighbor
                if (y > 0)
                {
                    Cells[x, y].Bottom = Cells[x, y - 1];
                }
                // Top most cells do not have top neighbor
                if (y < Size - 1)
                {
                    Cells[x, y].Top = Cells[x, y + 1];
                }

            }
		}
	}

	void LateUpdate() {

		Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		int x = (int)((pos.x));
		int y = (int)((pos.y));

		// Check if we are filling or erasing walls
		if (Input.GetMouseButtonDown(0))
		{
			if ((x > 0 && x < Cells.GetLength(0)) && (y > 0 && y < Cells.GetLength(1)))
			{
				if (Cells[x, y].Type == CellType.Blank)
				{
					Fill = true;
				}
				else
				{
					Fill = false;
				}
			}
		}

		// Left click draws/erases walls
		if (Input.GetMouseButton(0))
		{
			if (x != 0 && y != 0 && x != Size - 1 && y != Size - 1)
			{
				if ((x > 0 && x < Cells.GetLength(0)) && (y > 0 && y < Cells.GetLength(1)))
				{
					if (Fill)
					{
						Cells[x, y].SetType(CellType.Solid);
					}
					else
					{
						Cells[x, y].SetType(CellType.Blank);
					}
				}
			}
		}

		// Right click places liquid
		if (Input.GetMouseButton(1)) {
			if ((x > 0 && x < Cells.GetLength(0)) && (y > 0 && y < Cells.GetLength(1))) {
				Cells[x, y].AddLiquid(5);
			}
		}

		
    }


    IEnumerator DelayExecuteSim(float time)
    {
        yield return new WaitForSeconds(time);

        
        Vector3Int[] positions = new Vector3Int[Size * Size];
        TileBase[] tileArray = new TileBase[positions.Length];

        int posIndex = 0;

        // Determine sprite for each cell
        for (int cx = 0; cx < Cells.GetLength(0); cx++)
        {
            for (int cy = 0; cy < Cells.GetLength(1); cy++)
            {
                tileArray[cx * Size + cy] = Cells[cx, cy].CellUpdate(LiquidTilemap, WaterTiles, BlockTile);
                positions[posIndex] = new Vector3Int(cx, cy, 0);
                posIndex++;
            }
            
        }

        LiquidTilemap.SetTiles(positions, tileArray);

        yield return 0;

        // Run our liquid simulation 
        LiquidSimulator.Simulate(ref Cells);

		// Repeat
        yield return StartCoroutine(DelayExecuteSim(UpdateDelayTime));
    }

}
