using System;
using UnityEngine;

public class VoxelTile : MonoBehaviour
{
    public float VoxelSize = 0.1f;
    public int TilesSizeVoxel = 8;

    [Range(1, 100)]
    public int Weight = 50;

    public RotationType Rotation;
    public enum RotationType
    {
        OnlyRotation,
        TwoRotation,
        FourRotation
    }

    [HideInInspector] public byte[] ColorRight;
    [HideInInspector] public byte[] ColorLeft;
    [HideInInspector] public byte[] ColorForward;
    [HideInInspector] public byte[] ColorBack;

    public void CalculateSidesColors()
    {
        ColorRight = new byte[TilesSizeVoxel * TilesSizeVoxel];
        ColorLeft = new byte[TilesSizeVoxel * TilesSizeVoxel];
        ColorForward = new byte[TilesSizeVoxel * TilesSizeVoxel];
        ColorBack = new byte[TilesSizeVoxel * TilesSizeVoxel];

        for (int y = 0; y < TilesSizeVoxel; y++)
        {
            for (int i = 0; i < TilesSizeVoxel; i++)
            {
                ColorRight[y * TilesSizeVoxel + i] = GetTileColor(y, i, Direction.Right);
                ColorLeft[y * TilesSizeVoxel + i] = GetTileColor(y, i, Direction.Left);
                ColorForward[y * TilesSizeVoxel + i] = GetTileColor(y, i, Direction.Forward);
                ColorBack[y * TilesSizeVoxel + i] = GetTileColor(y, i, Direction.Back);
            }
        }
    }
    public void Rotate90()
    {
        transform.Rotate(0, 90, 0);

        byte[] colorsRightNew = new byte[TilesSizeVoxel * TilesSizeVoxel];
        byte[] colorsForwardNew = new byte[TilesSizeVoxel * TilesSizeVoxel];
        byte[] colorsLeftNew = new byte[TilesSizeVoxel * TilesSizeVoxel];
        byte[] colorsBackNew = new byte[TilesSizeVoxel * TilesSizeVoxel];

        for(int layer = 0; layer < TilesSizeVoxel; layer++)
        {
            for(int offset = 0; offset < TilesSizeVoxel; offset++)
            {
                colorsRightNew[layer * TilesSizeVoxel + offset] = ColorForward[layer * TilesSizeVoxel + TilesSizeVoxel - offset - 1];
                colorsForwardNew[layer * TilesSizeVoxel + offset] = ColorLeft[layer * TilesSizeVoxel + offset];
                colorsLeftNew[layer * TilesSizeVoxel + offset] = ColorBack[layer * TilesSizeVoxel + TilesSizeVoxel - offset - 1];
                colorsBackNew[layer * TilesSizeVoxel + offset] = ColorRight[layer * TilesSizeVoxel + offset];
            }
        }

        ColorRight = colorsRightNew;
        ColorLeft = colorsLeftNew ;
        ColorForward = colorsForwardNew;
        ColorBack =   colorsBackNew;
    }

    private byte GetTileColor(int verticaleLayer, int horizontalOffset, Direction direction)
    {
        MeshCollider meshCollider = GetComponentInChildren<MeshCollider>();

        Vector3 rayStart = Vector3.zero;
        Vector3 rayDir;

        float vox = VoxelSize;
        float half = VoxelSize/2;
        if (direction == Direction.Right)
        {
            rayStart = meshCollider.bounds.min +
                new Vector3(-half, 0, half + horizontalOffset * vox);
            rayDir = Vector3.right;
        }
        else if(direction == Direction.Forward)
        {
            rayStart = meshCollider.bounds.min +
                new Vector3(half + horizontalOffset * vox, 0, -half);
            rayDir = Vector3.forward;
        }
        else if(direction == Direction.Left)
        {
            rayStart = meshCollider.bounds.max +
                new Vector3(half, 0, -half - (TilesSizeVoxel - horizontalOffset - 1) * vox);
            rayDir = Vector3.left;
        }
        else if(direction == Direction.Back)
        {
            rayStart = meshCollider.bounds.max +
                new Vector3(-half - (TilesSizeVoxel - horizontalOffset - 1) * vox, 0, half);
            rayDir = Vector3.back;
        }
        else
        {
            return 0;
        }
        rayStart.y = meshCollider.bounds.min.y + half + verticaleLayer * vox;

        //Debug.DrawRay(rayStart, direction * 0.1f, Color.blue, 2);

        if (Physics.Raycast(new Ray(rayStart, rayDir), out RaycastHit _hit, VoxelSize))
        {
            byte colorIndex = (byte)(_hit.textureCoord.x * 256);

            if (colorIndex == 0) Debug.LogWarning("Color Index == 0");

            return colorIndex;
        }
        return 0;
    }
}
