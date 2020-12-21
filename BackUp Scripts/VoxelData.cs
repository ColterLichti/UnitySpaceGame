using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Easy way to reference faces from an array
// Face arrays are ALWAYS ordered according to this enum!
// NORTH is Unity Forward (Z+)!
public enum FaceDirection
{
    NORTH = 0,
    EAST = 1,
    SOUTH = 2,
    WEST = 3,
    UP = 4,
    DOWN = 5
}

public struct PureVoxelData
{
    public Vector3[][] CardinalFaces { get; set; }
}

public struct PartialVoxelData
{
    // Arrays representing the partial (non cube-bound) data of this voxel
    Vector3[] PartialVertices;
    int[] PartialTraingles;
    Vector2[] PartialUVs;
}

public struct VoxelTypeData
{
    public bool bIsFullyPure { get; set; }
    public int PureBaseID { get; set; }
    public int PartialID { get; set; }

    public bool bCanRotateX { get; set; }
    public bool bCanRotateY { get; set; }
    public bool bCanRotateZ { get; set; }

    public bool[] FaceCullingMask { get; set; }
}

public class VoxelData : MonoBehaviour
{
    // I hope to load these in eventually!
    public static readonly PureVoxelData[] PureBases = new PureVoxelData[]
    {
        // BEGIN - Default Cube
        new PureVoxelData()
        {
            CardinalFaces = new Vector3[][]
            {
                // BEGIN - North Face
                new Vector3[]
                {
                    new Vector3( 0.5f,  0.5f,  0.5f), // Top Left
                    new Vector3(-0.5f,  0.5f,  0.5f), // Top Right
                    new Vector3(-0.5f, -0.5f,  0.5f), // Bottom Right
                    new Vector3( 0.5f, -0.5f,  0.5f) // Bottom Left
                },
                //END - North Face

                //BEGIN - East Face
                new Vector3[]
                {
                    new Vector3( 0.5f,  0.5f, -0.5f), // Top Left
                    new Vector3( 0.5f,  0.5f,  0.5f), // Top Right
                    new Vector3( 0.5f, -0.5f,  0.5f), // Bottom Right
                    new Vector3( 0.5f, -0.5f, -0.5f) // Bottom Left
                },
                //END - East Face

                // BEGIN - South Face
                new Vector3[]
                {
                    new Vector3(-0.5f,  0.5f, -0.5f), // Top Left
                    new Vector3( 0.5f,  0.5f, -0.5f), // Top Right
                    new Vector3( 0.5f, -0.5f, -0.5f), // Bottom Right
                    new Vector3(-0.5f, -0.5f, -0.5f) // Bottom Left
                },
                // END - South Face

                // BEGIN - West Face
                new Vector3[]
                {
                    new Vector3(-0.5f,  0.5f,  0.5f), // Top Left
                    new Vector3(-0.5f,  0.5f, -0.5f), // Top Right
                    new Vector3(-0.5f, -0.5f, -0.5f), // Bottom Right
                    new Vector3(-0.5f, -0.5f,  0.5f), // Bottom Left
                },
                // END - West Face

                // BEGIN - Up Face
                new Vector3[]
                {
                    new Vector3(-0.5f,  0.5f,  0.5f), // Top Left
                    new Vector3( 0.5f,  0.5f,  0.5f), // Top Right
                    new Vector3( 0.5f,  0.5f, -0.5f), // Bottom Right
                    new Vector3(-0.5f,  0.5f, -0.5f) // Bottom Left
                },
                // END - Up Face

                // BEGIN - Down Face
                new Vector3[]
                {
                    new Vector3(-0.5f, -0.5f, -0.5f), // Top Left
                    new Vector3( 0.5f, -0.5f, -0.5f), // Top Right
                    new Vector3( 0.5f, -0.5f,  0.5f), // Bottom Right
                    new Vector3(-0.5f, -0.5f,  0.5f) // Bottom Left
                }
                // END - Down Face
            }
        }
        // END - Default Cube
    };

    public static readonly PartialVoxelData[] PartialVoxels;

    public static readonly VoxelType[] MasterVoxelList = new VoxelType[]
    {
        // 0 - DEFAULT CUBE
        new VoxelType()
        {
            bIsFullyPure = true,
            PureBaseID = 0,
            PartialID = 0,
            bCanRotateX = false,
            bCanRotateY = false,
            bCanRotateZ = false,
            FaceCullingMask = new bool[]
            {
                true,
                true,
                true,
                true,
                true,
                true
            }
        }
    };

    // Offset vectors representing the cardinal voxel neighbour locations
    public static Vector3Int[] NeighbourOffsets { get; } = new Vector3Int[]
    {
        new Vector3Int( 0, 0, 1),   // NORTH
        new Vector3Int( 1, 0, 0),   // EAST
        new Vector3Int( 0, 0,-1),   // SOUTH
        new Vector3Int(-1, 0, 0),   // WEST
        new Vector3Int( 0, 1, 0),   // UP
        new Vector3Int( 0,-1, 0)    // DOWN
    };

    public static int VOXEL_SIZE { get; } = 1;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
