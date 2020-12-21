using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Represents a chunk of voxel geometry
 * breaking the geometry into chunks means less work when edits happen
 */

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class Chunk : MonoBehaviour
{
    // Does chunk need a rebuild?
    public bool bNeedsUpdate { get; set; }
    public MeshFilter ProceduralMesh { get; set; }
    public MeshCollider Collider { get; set; }

    // The system that owns this chunk
    public SealedVoxelSystem Owner { get; set; }

    // Tracks which voxels are rendered in this chunk
    private Dictionary<Vector3Int, Voxel> ChunkVoxelMap { get; set; }

    public Chunk()
    {
        ChunkVoxelMap = new Dictionary<Vector3Int, Voxel>();
        bNeedsUpdate = false;
    }

    // Can call anytime voxel data is changed
    public void BuildChunk(uint CurrentCheck)
    {
        if(bNeedsUpdate)
        {
            PreprocessChunk(CurrentCheck);
            GenerateMesh();
            bNeedsUpdate = false;
        }
    }

    public void AddVoxel(Vector3Int location, Voxel voxel)
    {
        ChunkVoxelMap.Add(location, voxel);
        bNeedsUpdate = true;
    }

    public void RemoveVoxel(Vector3Int location)
    {
        bool voxRemoved = ChunkVoxelMap.Remove(location);
        if (voxRemoved)
            bNeedsUpdate = true;
    }




    // Walk through chunk data and look for faces that can be culled
    void PreprocessChunk(uint currentCheck)
    {
        foreach(KeyValuePair<Vector3Int, Voxel> entry in ChunkVoxelMap)
        {
            Vector3Int location = entry.Key;
            Voxel vox = entry.Value;

            neighbourCheckVoxel(currentCheck, location, vox);
        }
    }

    // Check a voxel for all possible neighbour voxels
    void neighbourCheckVoxel(uint currentCheck, Vector3Int location, Voxel voxel)
    {
        int occlusionCounter = 0;

        if(voxel is GeometricVoxel)
        {
            GeometricVoxel vox = voxel as GeometricVoxel;

            for (int i = 0; i < vox.FaceVisibility.Length; i++)
            {
                if(vox.FaceCullingMask[i] && vox.FaceCheckID[i] != currentCheck)
                {
                    bool wasNeighbour = NeighbourCheckFace(currentCheck, VoxelDataLibrary.NeighbourOffsets[i] + location, vox, i);

                    if (wasNeighbour)
                        occlusionCounter++;
                }
            }

            if(occlusionCounter == VoxelDataLibrary.VOXEL_FACES)
            {
                vox.bIsFullyOccluded = true;
            }
        }

    }

    // Check if voxel face has a neighbour and can be culled
    bool NeighbourCheckFace(uint currentCheck, Vector3Int neighbourLocation, GeometricVoxel current, int currentFace)
    {
        bool wasNeighbour = false;

        if(Owner.MasterHasVoxel(neighbourLocation))
        {
            Voxel temp = Owner.GetVoxelFromMaster(neighbourLocation);
            if(temp is GeometricVoxel)
            {
                GeometricVoxel neighbour = temp as GeometricVoxel;
                int neighbourFace = InvertFaceID(currentFace);

                if(neighbour.FaceCullingMask[neighbourFace] && neighbour.FaceCheckID[neighbourFace] != currentCheck)
                {
                    wasNeighbour = true;

                    current.FaceVisibility[currentFace] = false;
                    neighbour.FaceVisibility[neighbourFace] = false;

                    current.FaceCheckID[currentFace] = currentCheck;
                    neighbour.FaceCheckID[neighbourFace] = currentCheck;
                }
            }
            else
            {
                current.FaceVisibility[currentFace] = true;
                current.FaceCheckID[currentFace] = currentCheck;
            }
        }

        return wasNeighbour;
    }

    // Inverts face ID to get neighbours adjacent face (IE -> Current: North, Neighbour: South)
    int InvertFaceID(int faceID)
    {
        switch (faceID)
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

    // Builds the chunks mesh, should always be called after preprocessing
    void GenerateMesh()
    {
        Mesh newMesh = new Mesh();
        newMesh.Clear();

        List<Vector3> Verts = new List<Vector3>();
        List<int> Tris = new List<int>();
        List<Vector2> UVs = new List<Vector2>();

        // Walk the map and start building a mesh
        foreach (KeyValuePair<Vector3Int, Voxel> entry in ChunkVoxelMap)
        {
            Vector3 location = ((Vector3) entry.Key) * VoxelDataLibrary.VOXEL_SIZE;
            Voxel voxel = entry.Value;

            AddVoxelDataToLists(location, voxel, Verts, Tris, UVs);
        }

        newMesh.vertices = Verts.ToArray();
        newMesh.triangles = Tris.ToArray();
        newMesh.uv = UVs.ToArray();

        newMesh.RecalculateNormals();
        newMesh.RecalculateTangents();
        newMesh.RecalculateBounds();

        if(ProceduralMesh == null)
        {
            ProceduralMesh = GetComponent<MeshFilter>();
        }

        if (Collider == null)
        {
            Collider = GetComponent<MeshCollider>();
        }

        ProceduralMesh.mesh = newMesh;
        Collider.sharedMesh = newMesh;
    }

    // Add any geometry this voxel might posses to the mesh lists
    private void AddVoxelDataToLists(Vector3 location, Voxel voxel, List<Vector3> verts, List<int> tris, List<Vector2> uvs)
    {
        if(voxel is GeometricVoxel)
        {
            GeometricVoxel vox = voxel as GeometricVoxel;
            // Add data for all voxel cardinal(NESWUD) faces
            for (int i = 0; i < vox.FaceVisibility.Length; i++)
            {
                // If the face shows through the mask and the face is marked visible
                // Add the parts of the voxel that should be rendered to the lists
                if (vox.FaceCullingMask[i] && vox.FaceVisibility[i])
                {
                    tris.Add(0 + verts.Count);
                    tris.Add(1 + verts.Count);
                    tris.Add(2 + verts.Count);
                    tris.Add(2 + verts.Count);
                    tris.Add(3 + verts.Count);
                    tris.Add(0 + verts.Count);

                    Vector3[] tempVerts = VoxelDataLibrary.PureBases[vox.BaseID].CardinalFaces[i];

                    for (int j = 0; j < tempVerts.Length; j++)
                    {
                        verts.Add(tempVerts[j] + (VoxelDataLibrary.VOXEL_SIZE * location));
                    }

                    uvs.Add(new Vector2(0, 0));
                    uvs.Add(new Vector2(1, 0));
                    uvs.Add(new Vector2(1, 1));
                    uvs.Add(new Vector2(0, 1));
                }
            }

        }

        // Render partial voxels if not fully occluded
        if(voxel is PartialVoxel)
        {
            PartialVoxel vox = voxel as PartialVoxel;

            if(!vox.bIsFullyOccluded)
            {
                int startIndex = verts.Count;

                Vector3[] tempVerts = VoxelDataLibrary.VoxelModels[vox.ModelID].PartialVertices;

                for (int i = 0; i < tempVerts.Length; i++)
                {
                    verts.Add((Quaternion.Euler(vox.GetRotation()) * tempVerts[i]) + (VoxelDataLibrary.VOXEL_SIZE * location));
                }

                int[] tempTris = VoxelDataLibrary.VoxelModels[vox.ModelID].PartialTriangles;

                for (int i = 0; i < tempTris.Length; i++)
                {
                    tris.Add(startIndex + tempTris[i]);
                }

                uvs.AddRange(VoxelDataLibrary.VoxelModels[vox.ModelID].PartialUVs);
            }
        }
    }

}
