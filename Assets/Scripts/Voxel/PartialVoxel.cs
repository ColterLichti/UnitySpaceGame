using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Voxel class representing a voxel that has both complex geometry and full face features
 * It can also be thought of as a voxel whos geometry only fills part of it's total volume,
 * hence the name, "partial".
 * Because of their likelyhood for dissymetry, partials are allowed to have rotation freedom
 */
public class PartialVoxel : GeometricVoxel
{

    public int ModelID { get; set; }
    public bool bCanRotateX { get; set; }
    public bool bCanRotateY { get; set; }
    public bool bCanRotateZ { get; set; }

    private Vector3 rotation = new Vector3(0,0,0);

    public PartialVoxel() : base()
    {
        InitVoxel();
    }

    public PartialVoxel(VoxelMetadata data) : base()
    {
        InitVoxel(data);
    }

    private void InitVoxel(VoxelMetadata? data = null)
    {
        // Copy data if present
        if (data != null)
        {
            VoxelMetadata voxData = data.Value;

            // Partial voxels may have some rotational freedom
            bCanRotateAtAll = voxData.bCanRotateX || voxData.bCanRotateY || voxData.bCanRotateZ;

            bCanRotateX = voxData.bCanRotateX;
            bCanRotateY = voxData.bCanRotateY;
            bCanRotateZ = voxData.bCanRotateZ;

            Array.Copy(data.Value.FaceCullingMask, FaceCullingMask, FaceCullingMask.Length);
            Array.Copy(FaceCullingMask, FaceVisibility, FaceCullingMask.Length);
            BaseID = voxData.BaseID;
            ModelID = voxData.ModelID;
        }
    }

    public void AddRotation(Vector3 rotation)
    {
        if(bCanRotateAtAll)
        {
            if (bCanRotateX)
                this.rotation.x += rotation.x;

            if (bCanRotateY)
                this.rotation.y += rotation.y;

            if (bCanRotateZ)
                this.rotation.z += rotation.z;


            this.rotation = SnapVector(this.rotation);
            RotateValues();
        }
    }

    public void SetRotation(Vector3 rotation)
    {
        if (bCanRotateAtAll)
        {
            if (bCanRotateX)
                this.rotation.x = rotation.x;

            if (bCanRotateY)
                this.rotation.y = rotation.y;

            if (bCanRotateZ)
                this.rotation.z = rotation.z;


            this.rotation = SnapVector(this.rotation);
            RotateValues();
        }
    }

    public Vector3 GetRotation()
    {
        return rotation;
    }

    // Snap a vector (check VoxelDateLibrary, should be 90 deg increments)
    private Vector3 SnapVector(Vector3 toSnap)
    {

        float snapX = Mathf.RoundToInt(Mathf.Clamp(toSnap.x, 0.0f, 360.0f) / VoxelDataLibrary.ROTAION_SNAP) * VoxelDataLibrary.ROTAION_SNAP;
        float snapY = Mathf.RoundToInt(Mathf.Clamp(toSnap.y, 0.0f, 360.0f) / VoxelDataLibrary.ROTAION_SNAP) * VoxelDataLibrary.ROTAION_SNAP;
        float snapZ = Mathf.RoundToInt(Mathf.Clamp(toSnap.z, 0.0f, 360.0f) / VoxelDataLibrary.ROTAION_SNAP) * VoxelDataLibrary.ROTAION_SNAP;

        return new Vector3(snapX, snapY, snapZ);
    }

    // Rotate face vlaues to align with voxel rotation
    private void RotateValues()
    {
        // New arrays for transpose
        bool[] newCullingMask = new bool[VoxelDataLibrary.VOXEL_FACES];
        bool[] newVisibility = new bool[VoxelDataLibrary.VOXEL_FACES];
        uint[] newCheckID = new uint[VoxelDataLibrary.VOXEL_FACES];

        for (int i = 0; i < VoxelDataLibrary.VOXEL_FACES; i++)
        {
            // Rotate face normal by voxel rotation
            Vector3 norm = VoxelDataLibrary.NeighbourOffsets[i];
            Quaternion quatRotation = Quaternion.Euler(rotation);
            Vector3 rotated = quatRotation * norm;

            // Convert the rotated face normal to it's corresponding face index
            int newIndex = VectorToIndex(new Vector3Int(Mathf.RoundToInt(rotated.x), Mathf.RoundToInt(rotated.y), Mathf.RoundToInt(rotated.z)));

            // Apply the old values to the new arrays
            // Note: Old array, old index; New array, new index
            newCullingMask[newIndex] = FaceCullingMask[i];
            newVisibility[newIndex] = FaceVisibility[i];
            newCheckID[newIndex] = FaceCheckID[i];
        }

        // Apply the new arrays back tot he originals to complete the transpose
        FaceCullingMask = newCullingMask;
        FaceVisibility = newVisibility;
        FaceCheckID = newCheckID;
    }

    // Map face normals to their respective face ID
    private int VectorToIndex(Vector3Int vec)
    {
        int index = 0;

        if(vec == new Vector3Int(0, 0, 1))
        {
            index = 0;
        }
        else if (vec == new Vector3Int(1, 0, 0))
        {
            index = 1;
        }
        else if (vec == new Vector3Int(0, 0, -1))
        {
            index = 2;
        }
        else if (vec == new Vector3Int(-1, 0, 0))
        {
            index = 3;
        }
        else if (vec == new Vector3Int(0, 1, 0))
        {
            index = 4;
        }
        else if (vec == new Vector3Int(0, -1, 0))
        {
            index = 5;
        }

        return index;
    }
}