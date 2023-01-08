using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class Grid : MonoBehaviour {

	int Width = 64;
	int Height = 64;

	public Cell[,] Cells;

	public LiquidSimulator LiquidSimulator;

	public Cell CellPrefab;

	public Tilemap LiquidTilemap;
	public RuleTile tileWater;
	public RuleTile tileBlock;

    bool Fill;

	void Awake() {

		// Generate our cells 
		CreateGrid ();

		// Initialize the liquid simulator
		if(LiquidSimulator == null)
			LiquidSimulator = new LiquidSimulator();

		LiquidSimulator.Initialize (Cells);
	}

	void CreateGrid() {

		Cells = new Cell[Width, Height];

		// Cells
		for (int x = 0; x < Width; x++)
		{
			for (int y = 0; y < Height; y++)
			{
				LiquidTilemap.SetTile(new Vector3Int(x,y,0), null);
				
				Cell cell = new Cell();

				cell.Set (x, y);

				// Add border
				if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
				{
					LiquidTilemap.SetTile(new Vector3Int(x, y), tileBlock);
					cell.SetType(CellType.Solid);
				}
				
                Cells [x, y] = cell;
			}
		}
		UpdateNeighbors ();
	}

	// Sets neighboring cell references
	void UpdateNeighbors() {
		for (int x = 0; x < Width; x++) {

			for (int y = 0; y < Height; y++) {

				// Left most cells do not have left neighbor
				if (x > 0 )
				{
					Cells[x, y].Left = Cells [x - 1, y];
				}
				// Right most cells do not have right neighbor
				if (x < Width - 1)
				{
					Cells[x, y].Right = Cells [x + 1, y];
				}
                // bottom most cells do not have bottom neighbor
                if (y > 0)
                {
                    Cells[x, y].Bottom = Cells[x, y - 1];
                }
                // Top most cells do not have top neighbor
                if (y < Height - 1)
                {
                    Cells[x, y].Top = Cells[x, y + 1];
                }

            }
		}
	}

	void Update() {

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
			if (x != 0 && y != 0 && x != Width - 1 && y != Height - 1)
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

		// Run our liquid simulation 
		LiquidSimulator.Simulate(ref Cells);

		//Vector3Int[] tilePos;
		//TileBase[] tileArray;

		// Update each cell
		for (int cx = Cells.GetLength(0) - 1; cx >= 0; cx--)
        {
            for (int cy = Cells.GetLength(1) - 1; cy >= 0; cy--)
            {
                Cells[cx, cy].CellUpdate(LiquidTilemap, tileWater, tileBlock);
            }
        }


		//LiquidTilemap.SetTiles(tilePos, tileArray);
    }

}
