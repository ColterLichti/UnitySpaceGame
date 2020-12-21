using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voxel
{
    public int ID { get; set; }
    public VoxelType Type { get; set; }
    public Vector3 Rotation { get; set; } = Vector3.zero;

    public bool bIsFullyOccluded { get; set; } = false;
    public bool[] FaceVisibility { get; set; }
    public uint[] FaceCheckID { get; set; }

    private static int IDCounter = 0;
    public Voxel(int typeID)
    {
        this.Type = VoxelData.MasterVoxelList[typeID];
        FaceVisibility = new bool[Type.FaceCullingMask.Length];
        Array.Copy(Type.FaceCullingMask, FaceVisibility, FaceVisibility.Length);
        FaceCheckID = new uint[FaceVisibility.Length];
        IncrementID();
    }

    public Voxel(VoxelType type)
    {
        this.Type = type;
        FaceVisibility = new bool[type.FaceCullingMask.Length];
        Array.Copy(type.FaceCullingMask, FaceVisibility, type.FaceCullingMask.Length);
        FaceCheckID = new uint[FaceVisibility.Length];
        IncrementID();
    }

    void IncrementID()
    {
        this.ID = IDCounter;
        IDCounter++;
    }
}
