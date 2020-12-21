using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]

public class VoxelComponent : MonoBehaviour
{

    [SerializeField] private GameObject ChunkPrefab;

    public MeshFilter ProceduralMesh { get; set; }
    public MeshCollider Collider { get; set; }
    public Dictionary<Vector3Int, Voxel> VoxelMap { get; set; } = new Dictionary<Vector3Int, Voxel>();

    private uint CurrentCheckID = 0;

    // Start is called before the first frame update
    void Start()
    {
        ProceduralMesh = GetComponent<MeshFilter>();
        Collider = GetComponent<MeshCollider>();

        AddVoxel(new Vector3Int(0, 0, 0), 0);
        AddVoxel(new Vector3Int(1, 0, 0), 0);
        AddVoxel(new Vector3Int(0, 1, 0), 0);
        AddVoxel(new Vector3Int(1, 1, 0), 0);

        AddVoxel(new Vector3Int(0, 0, 1), 0);
        AddVoxel(new Vector3Int(1, 0, 1), 0);
        AddVoxel(new Vector3Int(0, 1, 1), 0);
        AddVoxel(new Vector3Int(1, 1, 1), 0);

        PreprocessVoxels();
        GenerateMesh();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // ---------- Add Voxel Convienence Overloads ----------
    public void AddVoxel(Vector3Int localGrid, Voxel voxel)
    {
        VoxelMap.Add(localGrid, voxel);
    }

    public void AddVoxel(Vector3Int localGrid, int typeID)
    {
        VoxelMap.Add(localGrid, new Voxel(typeID));
    }

    public void AddVoxel(Vector3Int localGrid, VoxelType type)
    {
        VoxelMap.Add(localGrid, new Voxel(type));
    }

    // ---------- Get Voxel Convienence Overloads ----------


    // ---------- Remove Voxel Convienence Overloads ----------

    // ---------- Mesh Builder ----------

    void PreprocessVoxels()
    {
        CurrentCheckID++;

        foreach (KeyValuePair<Vector3Int, Voxel> entry in VoxelMap)
        {
            Vector3Int location = entry.Key * VoxelData.VOXEL_SIZE;
            Voxel voxel = entry.Value;

            NeighbourCheckVoxel(location, voxel);
        }
    }

    void NeighbourCheckVoxel(Vector3Int location, Voxel voxel)
    {
        int occlusionCounter = 0;

        for (int i = 0; i < voxel.FaceVisibility.Length; i++)
        {
            // If voxel face can be culled/occluded and hasn't been checked already
            if (voxel.Type.FaceCullingMask[i])
            {
                if(voxel.FaceCheckID[i] != CurrentCheckID)
                {
                    bool wasNeighbour = NeighbourCheckFace(VoxelData.NeighbourOffsets[i] + location, voxel, i);
                    if (wasNeighbour)
                    {
                        occlusionCounter++;
                    }
                }
            }
        }

        if(occlusionCounter == voxel.FaceVisibility.Length)
        {
            voxel.bIsFullyOccluded = true;
        }
    }

    // Handles face checking precess between neighbour faces
    // Return true if an occluding neighbour was there
    // If this value is true 6 times on 1 voxel it doesn't need to be rendered!
    bool NeighbourCheckFace(Vector3Int neighbourLocation, Voxel current, int currentFace)
    {
        bool bWasNeighbour = false;
        Voxel neighbour = null;
        VoxelMap.TryGetValue(neighbourLocation, out neighbour);
        // Neighbour is found
        if(neighbour != null)
        {
            int neighbourFace = InvertFaceID(currentFace);

            if (neighbour.Type.FaceCullingMask[neighbourFace])
            {
                if(neighbour.FaceCheckID[neighbourFace] != CurrentCheckID)
                {
                    // Flag faces with a cullable neighbour face
                    // This means the neighbour voxel occludes the current
                    bWasNeighbour = true;
                
                    // Turn off faces as they are against each other
                    current.FaceVisibility[currentFace] = false;
                    neighbour.FaceVisibility[neighbourFace] = false;

                    // Update checks to avoid double work
                    current.FaceCheckID[currentFace] = CurrentCheckID;
                    neighbour.FaceCheckID[neighbourFace] = CurrentCheckID;
                }
            }
        }
        // Neighbour not found
        else
        {
            // Show the face and mark this voxel as checked
            current.FaceVisibility[currentFace] = true;
            current.FaceCheckID[currentFace] = CurrentCheckID;
        }

        return bWasNeighbour;
    }

    // Inverts face ID to get neighbours adjacent face (IE -> Me: North, Neighbour: South)
    int InvertFaceID(int faceID)
    {
        switch(faceID)
        {
            case 0:
                return 2;

            case 1:
                return 3;

            case 2:
                return 0;

            case 3:
                return 1;

            case 4:
                return 5;

            case 5:
                return 4;

            default:
                return -1;
        }
    }

    void GenerateMesh()
    {
        Mesh newMesh = new Mesh();
        newMesh.Clear();

        List<Vector3> Verts = new List<Vector3>();
        List<int> Tris = new List<int>();
        List<Vector2> UVs = new List<Vector2>();

        // Walk the map and start building a mesh
        foreach (KeyValuePair<Vector3Int, Voxel> entry in VoxelMap)
        {
            Vector3 location = entry.Key * VoxelData.VOXEL_SIZE;
            Voxel voxel = entry.Value;

            AddVoxelDataToLists(location, voxel, Verts, Tris, UVs);
        }

        newMesh.vertices = Verts.ToArray();
        newMesh.triangles = Tris.ToArray();
        newMesh.uv = UVs.ToArray();

        newMesh.RecalculateNormals();
        newMesh.RecalculateTangents();
        newMesh.RecalculateBounds();

        ProceduralMesh.mesh = newMesh;
        Collider.sharedMesh = newMesh;
    }

    private void AddVoxelDataToLists(Vector3 location, Voxel voxel, List<Vector3> verts, List<int> tris, List<Vector2> uvs)
    {
        // Add data for all voxel cardinal(NESWUD) faces
        for (int i = 0; i < voxel.FaceVisibility.Length; i++)
        {
            // If the face shows through the mask and the face is marked visible
            // Add the parts of the voxel that should be rendered to the lists
            if(voxel.Type.FaceCullingMask[i] && voxel.FaceVisibility[i])
            {
                tris.Add(0 + verts.Count);
                tris.Add(1 + verts.Count);
                tris.Add(2 + verts.Count);
                tris.Add(2 + verts.Count);
                tris.Add(3 + verts.Count);
                tris.Add(0 + verts.Count);

                Vector3[] tempVerts = VoxelData.PureBases[0].CardinalFaces[i];

                for (int j = 0; j < tempVerts.Length; j++)
                {
                    verts.Add(tempVerts[j] + (VoxelData.VOXEL_SIZE * location));
                }

                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(1, 0));
                uvs.Add(new Vector2(1, 1));
                uvs.Add(new Vector2(0, 1));
            }
        }
    }
}
