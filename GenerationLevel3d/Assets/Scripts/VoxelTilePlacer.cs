using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VoxelTilePlacer : MonoBehaviour
{
    public List<VoxelTile> tilePrafabs;
    public Vector2Int mapSize = new Vector2Int(10, 10);

    private VoxelTile[,] spawnedTiles;
    void Start()
    {
        spawnedTiles = new VoxelTile[mapSize.x, mapSize.y];
        foreach(VoxelTile tilePrefab in tilePrafabs)
        {
            tilePrefab.CalculateSidesColors();
        }

        int countBeforeAdding = tilePrafabs.Count;
        for (int i = 0; i < countBeforeAdding; i++)
        {
        VoxelTile clone;
            switch (tilePrafabs[i].Rotation)
            {
                case VoxelTile.RotationType.OnlyRotation:
                    break;
                case VoxelTile.RotationType.TwoRotations:
                    tilePrafabs[i].Weight /= 2;
                    if (tilePrafabs[i].Weight <= 0) tilePrafabs[i].Weight = 1;

                    clone = Instantiate(tilePrafabs[i], tilePrafabs[i].transform.position + Vector3.right, Quaternion.identity);
                    clone.Rotate90();
                    tilePrafabs.Add(clone);
                    break;
                case VoxelTile.RotationType.FourRotations:
                    tilePrafabs[i].Weight /= 4;
                    if (tilePrafabs[i].Weight <= 0) tilePrafabs[i].Weight = 1;

                    clone = Instantiate(tilePrafabs[i], tilePrafabs[i].transform.position + Vector3.right, Quaternion.identity);
                    clone.Rotate90();
                    tilePrafabs.Add(clone);

                    clone = Instantiate(tilePrafabs[i], tilePrafabs[i].transform.position + Vector3.right*2, Quaternion.identity);
                    clone.Rotate90();
                    clone.Rotate90();
                    tilePrafabs.Add(clone);

                    clone = Instantiate(tilePrafabs[i], tilePrafabs[i].transform.position + Vector3.right*3, Quaternion.identity);
                    clone.Rotate90();
                    clone.Rotate90();
                    clone.Rotate90();
                    tilePrafabs.Add(clone);
                    break;
            }
        }

        StartCoroutine(Generate());
    }

    public IEnumerator Generate()
    {
        for (int x = 1; x < mapSize.x - 1; x++)
        {
            for (int y = 1; y < mapSize.y - 1; y++)
            {
                yield return new WaitForSeconds(0.01f);

                PlaceTile(x, y);
            }
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            StopAllCoroutines();
            foreach(VoxelTile spawnedTile in spawnedTiles)
            {
                if (spawnedTile != null) Destroy(spawnedTile.gameObject);
            }
            StartCoroutine(Generate());
        }
    }
    public void PlaceTile(int x, int y)
    {
        List<VoxelTile> availableTiles = new List<VoxelTile>();

        foreach(VoxelTile tilePrefab in tilePrafabs)
        {
            if (CanAppendTile(spawnedTiles[x - 1, y], tilePrefab, Direction.Right) &&
                CanAppendTile(spawnedTiles[x + 1, y], tilePrefab, Direction.Left) &&
                CanAppendTile(spawnedTiles[x, y + 1], tilePrefab, Direction.Back) &&
                CanAppendTile(spawnedTiles[x, y - 1], tilePrefab, Direction.Forward))
            {
                availableTiles.Add(tilePrefab);
            }
        }
        if (availableTiles.Count == 0) return;
        // выбираем рандомный тайл из всех возможных тайлов и ставим его
        VoxelTile selectedTile = GetRandomTile(availableTiles); 
        Vector3 position = new Vector3(x, 0, y) * selectedTile.VoxelSize * selectedTile.TilesSizeVoxel;
        spawnedTiles[x, y] = Instantiate(selectedTile, position, selectedTile.transform.rotation);
    }
    private VoxelTile GetRandomTile(List<VoxelTile> availableTiles)
    {
        List<float> chances = new List<float>();
        for (int i = 0; i < availableTiles.Count; i++)
        {
            chances.Add(availableTiles[i].Weight);
        }

        float value = Random.Range(0, chances.Sum());
        float sum = 0;

        for (int i = 0; i < chances.Count; i++)
        {
            sum += chances[i];
            if (value < sum)
            {
                return availableTiles[i];
            }
        }
        return availableTiles[availableTiles.Count - 1];
    }
    private bool CanAppendTile(VoxelTile existingTile, VoxelTile tileToAppend, Direction direction)
    {
        if (existingTile == null) return true;
        if (direction == Direction.Left)
        {
            return Enumerable.SequenceEqual(existingTile.ColorsRight, tileToAppend.ColorsLeft);
        }
        else if (direction == Direction.Right)
        {
            return Enumerable.SequenceEqual(existingTile.ColorsLeft, tileToAppend.ColorsRight);
        }
        else if (direction == Direction.Back)
        {
            return Enumerable.SequenceEqual(existingTile.ColorsForward, tileToAppend.ColorsBack);
        }
        else if (direction == Direction.Forward)
        {
            return Enumerable.SequenceEqual(existingTile.ColorsBack, tileToAppend.ColorsForward);
        }
        else
        {
            Debug.LogError("Error in CanAppendTile - No direction tile");
            return false;
        }

    }
}
