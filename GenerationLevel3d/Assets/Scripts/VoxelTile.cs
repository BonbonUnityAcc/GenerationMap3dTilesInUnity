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
        TwoRotations,
        FourRotations
    }

    [HideInInspector] public byte[] ColorsRight;
    [HideInInspector] public byte[] ColorsLeft;
    [HideInInspector] public byte[] ColorsForward;
    [HideInInspector] public byte[] ColorsBack;

    public void CalculateSidesColors()
    {
        ColorsRight = new byte[TilesSizeVoxel * TilesSizeVoxel];
        ColorsLeft = new byte[TilesSizeVoxel * TilesSizeVoxel];
        ColorsForward = new byte[TilesSizeVoxel * TilesSizeVoxel];
        ColorsBack = new byte[TilesSizeVoxel * TilesSizeVoxel];

        for (int y = 0; y < TilesSizeVoxel; y++)
        {
            for (int i = 0; i < TilesSizeVoxel; i++)
            {
                ColorsRight[y * TilesSizeVoxel + i] = GetTileColor(y, i, Direction.Right);
                ColorsLeft[y * TilesSizeVoxel + i] = GetTileColor(y, i, Direction.Left);
                ColorsForward[y * TilesSizeVoxel + i] = GetTileColor(y, i, Direction.Forward);
                ColorsBack[y * TilesSizeVoxel + i] = GetTileColor(y, i, Direction.Back);
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
                colorsRightNew[layer * TilesSizeVoxel + offset] = ColorsForward[layer * TilesSizeVoxel + TilesSizeVoxel - offset - 1];
                colorsForwardNew[layer * TilesSizeVoxel + offset] = ColorsLeft[layer * TilesSizeVoxel + offset];
                colorsLeftNew[layer * TilesSizeVoxel + offset] = ColorsBack[layer * TilesSizeVoxel + TilesSizeVoxel - offset - 1];
                colorsBackNew[layer * TilesSizeVoxel + offset] = ColorsRight[layer * TilesSizeVoxel + offset];
            }
        }

        ColorsRight = colorsRightNew;
        ColorsLeft = colorsLeftNew ;
        ColorsForward = colorsForwardNew;
        ColorsBack =   colorsBackNew;
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

        //Debug.DrawRay(rayStart, rayDir, Color.blue, 5);

        if (Physics.Raycast(new Ray(rayStart, rayDir), out RaycastHit _hit, VoxelSize))
        {
            byte colorIndex = (byte)(_hit.textureCoord.x * 256);

            if (colorIndex == 0) Debug.LogWarning("Color Index == 0");

            return colorIndex;
        }
        return 0;
    }
}
