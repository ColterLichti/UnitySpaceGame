using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A voxel that will render geomtry during generation
// Opposed to placeholders that do not
public abstract class GeometricVoxel : Voxel
{
    public bool bCanRotateAtAll { get; set; } = false;

    public bool bIsFullyOccluded { get; set; }

    public bool[] FaceVisibility { get; set; } = new bool[VoxelDataLibrary.VOXEL_FACES];

    public bool[] FaceCullingMask { get; set; } = new bool[VoxelDataLibrary.VOXEL_FACES];

    public uint[] FaceCheckID { get; set; } = new uint[VoxelDataLibrary.VOXEL_FACES];

    // The base data this voxel uses
    public int BaseID { get; set; }

    // Call parent contructor to incremenet ID
    public GeometricVoxel() : base()
    {
        // By default nothing will render
        FillArray(FaceVisibility, true);
        FillArray(FaceCullingMask, false);
    }

    protected void FillArray(bool[] arr, bool value)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = value;
        }
    }
}
