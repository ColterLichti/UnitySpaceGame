using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Voxel
{
    private static uint VoxelCounter = 0;

    public string ID { get; private set; }
    
    public Voxel()
    {
        this.ID = "Voxel - ID#: " + VoxelCounter;
        VoxelCounter++;
    }

}
