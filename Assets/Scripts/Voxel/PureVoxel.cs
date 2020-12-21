using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PureVoxel : GeometricVoxel
{ 

    public PureVoxel() : base()
    {
        InitVoxel();
    }

    public PureVoxel(VoxelMetadata data) : base ()
    {
        InitVoxel(data);
    }

    private void InitVoxel(VoxelMetadata? data = null)
    {
        // Pure voxels don't rotate
        bCanRotateAtAll = false;

        // Copy data if present
        if(data != null)
        {
            Array.Copy(data.Value.FaceCullingMask, FaceCullingMask, FaceCullingMask.Length);
            Array.Copy(FaceCullingMask, FaceVisibility, FaceCullingMask.Length);
            BaseID = data.Value.BaseID;
        }
    }
}
