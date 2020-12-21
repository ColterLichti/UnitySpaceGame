using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ChunkMapEntry
{
    public Chunk Script { get; set; }
    public GameObject Prefab { get; set; }
}

public class SealedVoxelSystem : MonoBehaviour
{

    [SerializeField] private GameObject ChunkPrefab;

    private Dictionary<Vector3Int, Voxel> MasterVoxelMap;
    private Dictionary<Vector3Int, ChunkMapEntry> ChunkMap;
    private uint CurrentCheckID = 0;

    // Start is called before the first frame update
    void Start()
    {
        MasterVoxelMap = new Dictionary<Vector3Int, Voxel>();
        ChunkMap = new Dictionary<Vector3Int, ChunkMapEntry>();
        DateTime before = DateTime.Now;
        //int cube = 10;
        //for (int i = 0; i < cube; i++)
        //{
        //    for (int j = 0; j < cube; j++)
        //    {
        //        for (int k = 0; k < cube; k++)
        //        {
        //            AddVoxel(new Vector3Int(i, j, k), 0);
        //        }
        //    }

        //}


        AddVoxel(new Vector3Int(0, 0, 0), 1);
        AddVoxel(new Vector3Int(0, 0, 1), 0);
        //AddVoxel(new Vector3Int(0, 1, 0), 0);
        //AddVoxel(new Vector3Int(1, 1, 0), 0);
        //AddVoxel(new Vector3Int(2, 1, 0), 0);
        //AddVoxel(new Vector3Int(0, 0, 1), 0);
        //AddVoxel(new Vector3Int(1, 0, 1), 0);
        //AddVoxel(new Vector3Int(0, 1, 1), 0);
        //AddVoxel(new Vector3Int(1, 1, 1), 0);

        
        RebuildChunks();        
        DateTime after = DateTime.Now;
        TimeSpan duration = after.Subtract(before);
        Debug.Log("Duration in milliseconds: " + duration.Milliseconds);

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void AddVoxel(Vector3Int localGrid, Voxel voxel)
    {
        MasterVoxelMap.Add(localGrid, voxel);

        Vector3Int chunkIndex = GetChunkIndex(localGrid);
        ChunkMapEntry mapEntry;
        if(ChunkMap.ContainsKey(chunkIndex))
        {
            ChunkMap.TryGetValue(chunkIndex, out mapEntry);
        }
        else
        {
            mapEntry = new ChunkMapEntry();
            mapEntry.Prefab = Instantiate(ChunkPrefab, this.transform, false);
            mapEntry.Script = mapEntry.Prefab.GetComponentInChildren<Chunk>();
            mapEntry.Script.Owner = this;
            ChunkMap.Add(chunkIndex, mapEntry);
        }

        mapEntry.Script.AddVoxel(localGrid, voxel);

        //RebuildChunks();
    }

    public void AddVoxel(Vector3Int localGrid, int typeID)
    {
        AddVoxel(localGrid, VoxelDataLibrary.VoxelTypes[typeID]);
    }

    public void AddVoxel(Vector3Int localGrid, VoxelMetadata data)
    {
        if(data.MetaType == VoxelMetaType.PURE)
        {
            PureVoxel voxel = new PureVoxel(data);
            AddVoxel(localGrid, voxel);
        }
        else if( data.MetaType == VoxelMetaType.PARTIAL)
        {
            PartialVoxel voxel = new PartialVoxel(data);
            voxel.SetRotation(new Vector3(0, 90f, 0));
            AddVoxel(localGrid, voxel);
        }
        else if( data.MetaType == VoxelMetaType.PLACEHOLDER)
        {
            PlaceholderVoxel voxel = new PlaceholderVoxel();
            AddVoxel(localGrid, voxel);
        }
    }

    public bool MasterHasVoxel(Vector3Int location)
    {
        return MasterVoxelMap.ContainsKey(location);
    }

    public Voxel GetVoxelFromMaster(Vector3Int location)
    {
        Voxel voxel = null;
        MasterVoxelMap.TryGetValue(location, out voxel);

        return voxel;
    }

    private void RebuildChunks()
    {
        CurrentCheckID++;

        foreach(KeyValuePair<Vector3Int, ChunkMapEntry> entry in ChunkMap)
        {
            ChunkMapEntry chunkEntry = entry.Value;

            chunkEntry.Script.BuildChunk(CurrentCheckID);
        }
    }

    private Vector3Int GetChunkIndex(Vector3Int localGrid)
    {
        Vector3 chunkIndex = ((Vector3)localGrid) / VoxelDataLibrary.CHUNK_SIZE;

        return new Vector3Int(Mathf.RoundToInt(chunkIndex.x), Mathf.RoundToInt(chunkIndex.y), Mathf.RoundToInt(chunkIndex.z));
    }
}
