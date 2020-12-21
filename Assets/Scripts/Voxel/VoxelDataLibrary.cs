using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Easy way to reference faces from an array
// Face arrays are ALWAYS ordered according to this enum!
// NORTH is Unity Forward (Z+)!
public enum CardinalFaceDirection
{
    NORTH = 0,
    EAST = 1,
    SOUTH = 2,
    WEST = 3,
    UP = 4,
    DOWN = 5
}

public enum VoxelMetaType
{
    PURE = 0,
    PARTIAL = 1,
    PLACEHOLDER = 2
}

public struct PureBaseData
{
    public Vector3[][] CardinalFaces { get; set; }
}

public struct ModelData
{
    // Arrays representing the partial (non cube-bound) data of this voxel
    public Vector3[] PartialVertices { get; set; }
    public int[] PartialTriangles { get; set; }
    public Vector2[] PartialUVs { get; set; }
}

public struct VoxelMetadata
{
    public VoxelMetaType MetaType { get; set; }

    public bool[] FaceCullingMask { get; set; }

    public bool bCanRotateX { get; set; }
    public bool bCanRotateY { get; set; }
    public bool bCanRotateZ { get; set; }

    public int BaseID { get; set; }

    public int ModelID { get; set; }
}


// Static data about voxels
public class VoxelDataLibrary : MonoBehaviour
{
    public static int VOXEL_FACES { get; } = 6;
    public static float VOXEL_SIZE { get; } = 1.0f;
    public static int CHUNK_SIZE { get; } = 32;
    public static float ROTAION_SNAP { get; set; } = 90.0f;

    public static Vector3Int[] NeighbourOffsets { get; } = new Vector3Int[]
    {
        new Vector3Int( 0, 0, 1),   // NORTH
        new Vector3Int( 1, 0, 0),   // EAST
        new Vector3Int( 0, 0,-1),   // SOUTH
        new Vector3Int(-1, 0, 0),   // WEST
        new Vector3Int( 0, 1, 0),   // UP
        new Vector3Int( 0,-1, 0)    // DOWN
    };

    public static PureBaseData[] PureBases { get; private set; } = new PureBaseData[]
    {
        // BEGIN - Default Cube
        new PureBaseData()
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

    public static ModelData[] VoxelModels { get; private set; } = new ModelData[]
    {
        // BEGIN - Default Ramp
        new ModelData()
        {
            PartialVertices = new Vector3[]
            {
                new Vector3( 0.5f, -0.5f,  0.5f),
                new Vector3(-0.5f, -0.5f,  0.5f),
                new Vector3( 0.5f,  0.5f,  0.5f),
                new Vector3(-0.5f,  0.5f,  0.5f),
                new Vector3( 0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f)
            },

            PartialTriangles = new int[]
            {
                2,0,4, 2,4,5, 2,5,3, 1,3,5
            },

            PartialUVs = new Vector2[]
            {
                new Vector2(0.374045f, 0.687091f),
                new Vector2(0.174301f, 0.484136f),
                new Vector2(0.374326f, 0.280485f),
                new Vector2(0.657006f, 0.280683f),
                new Vector2(0.656726f, 0.687289f),
                new Vector2(0.856751f, 0.484614f)
            }
        }
        // END - Default Ramp
    };

    public static VoxelMetadata[] VoxelTypes { get; set; } = new VoxelMetadata[]
    {
        // Default Cube
        new VoxelMetadata()
        {
            MetaType = VoxelMetaType.PURE,
            FaceCullingMask = new bool[]
            {
                true,
                true,
                true,
                true,
                true,
                true
            },
            bCanRotateX = false,
            bCanRotateY = false,
            bCanRotateZ = false,
            BaseID = 0,
            ModelID = -1
        },
        // Default Ramp
        new VoxelMetadata()
        {
            MetaType = VoxelMetaType.PARTIAL,
            FaceCullingMask = new bool[]
            {
                true,
                false,
                false,
                false,
                false,
                true
            },
            bCanRotateX = true,
            bCanRotateY = true,
            bCanRotateZ = true,
            BaseID = 0,
            ModelID = 0
        }
    };

}
