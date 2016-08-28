﻿using UnityEngine;
using System.Collections;

public class Chunk : MonoBehaviour{
    [HideInInspector] public int[,] chunkBiomes;
    [HideInInspector] public GameObject[,] hexagons       = new GameObject[chunkSize, chunkSize];
    [HideInInspector] public Structures[,] structuresData = new Structures[chunkSize, chunkSize];
    [HideInInspector] public GameObject[,] structures     = new GameObject[chunkSize, chunkSize];
    [HideInInspector] public int posX, posZ;        //Position of the chunk in HexCoords
    public static int chunkSize = 8;
    public static int worldConstant = 80;




    //WorldCoordinates
    public void setPosition(int x, int z)
    {
        posX = x * chunkSize;
        posZ = z * chunkSize;
        transform.position = Hexagon.getWorldPosition(posX, posZ);
        //print(x + ", " + z + ": " + transform.position);
    }


    /*
     * Must be called for generating the chunk
     */
    public void generate()
    {
        generateBioms();
        generateModel();
    }


    /*
     * Generates the biomes
     */
    public void generateBioms()
    {
        chunkBiomes = new int[chunkSize, chunkSize];

        for (int i = 0; i < chunkSize; i++)
        {
            for (int j = 0; j < chunkSize; j++)
            {
                //Select the correct terrain
                float x = World.instance.offsetX + (float)(i + posX) / worldConstant * 5f;
                float z = World.instance.offsetZ + (float)(j + posZ) / worldConstant * 5f;
                float biom = Mathf.PerlinNoise(x, z);

                if (biom > .85f)
                    chunkBiomes[i, j] = (int)Bioms.HighMountain;
                else if (biom > .8f)
                    chunkBiomes[i, j] = (int)Bioms.Mountain;
                else if (biom > .75f)
                {
                    if (Random.value > .3)
                        chunkBiomes[i, j] = (int)Bioms.Mountain;
                    else
                        chunkBiomes[i, j] = (int)Bioms.StonePlain;
                }
                else if (biom > .52f)
                    chunkBiomes[i, j] = Random.value > .5 ? (Random.value > .2 ? (int)Bioms.Plain : (int)Bioms.PlainDandelion) : (int)Bioms.Forest;
                else if (biom > .49f)
                    chunkBiomes[i, j] = Random.value > .2 ? (int)Bioms.Desert : (int)Bioms.DesertShell;
                else if (biom > .4f)
                    chunkBiomes[i, j] = Random.value > .05 ? (int)Bioms.Ocean : (int)Bioms.OceanMountain;
                else
                    chunkBiomes[i, j] = (int)Bioms.Ocean;

                structuresData[i, j] = World.instance.biomsData[chunkBiomes[i, j]].structure;
            }
        }
    }


    public void generateModel()
    {
        print(transform.position);
        for (int i = 0; i < chunkSize; i++)
        {
            for (int j = 0; j < chunkSize; j++)
            {
                BiomData biomData = World.instance.biomsData[chunkBiomes[i, j]];

                //Generate the ground
                GameObject g = Instantiate(World.instance.biomModels[(int)biomData.biomModel]);
                g.transform.position = transform.position + Hexagon.getWorldPosition(i, j);
                g.transform.parent = transform;
                hexagons[i, j] = g;

                //Generate the structure
                if ((int)biomData.structure != (int)Structures.None)
                {
                    structures[i, j] = Instantiate<GameObject>(World.instance.structureModels[(int)biomData.structure]);
                    structures[i, j].transform.position = transform.position + Hexagon.getWorldPosition(i, j);
                    structures[i, j].transform.parent = transform;
                }
                else
                    structures[i, j] = null;
            }
        }
    }



    public Bioms getBiomChunkCoords(int x, int z)
    {
        return (Bioms)chunkBiomes[x, z];
    }

    public Bioms getBiomWorldCoords(int x, int z)
    {
        Vector2Int pos = getPositionInChunk(x, z);
        try {
            return (Bioms)chunkBiomes[pos.x, pos.z];
        } catch (System.Exception e)
        {
            //print(chunkBiomes);
        }
        return Bioms.Desert;
    }

    public void changeBiomChunkCoords(int x, int z, Bioms newBiom)
    {
        chunkBiomes[x, z] = (int)newBiom;
        generateModel();
    }

    public void changeBiomGlobalCoords(int x, int z, Bioms newBiom)
    {
        Vector2Int pos = getPositionInChunk(x, z);
        changeBiomChunkCoords(pos.x, pos.z, newBiom);
    }

    public GameObject getStructureGameObjectChunkCoords(int x, int z)
    {
        return structures[x, z];
    }

    public GameObject getStructureGameObjectGlobalCoords(int x, int z)
    {
        Vector2Int pos = getPositionInChunk(x, z);
        return structures[x, z];
    }

    public Structures getStructureChunkCoords(int x, int z)
    {
        return structuresData[x, z];
    }

    public Structures getStructureGlobalCoords(int x, int z)
    {
        Vector2Int pos = getPositionInChunk(x, z);
        return structuresData[x, z];
    }

    public void changeStructureChunkCoords(int x, int z, Structures newStructure)
    {
        if(structures[x, z] != null)
            Destroy(structures[x, z]);

        structures[x, z] = Instantiate(World.instance.structureModels[(int)newStructure]);
        structures[x, z].transform.SetParent(gameObject.transform);
        structures[x, z].transform.position = Hexagon.getWorldPosition(x, z);

        structuresData[x, z] = newStructure;
    }

    public void changeStructureGlobalCoords(int x, int z, Structures newStructure)
    {
        Vector2Int pos = getPositionInChunk(x, z);
        changeStructureChunkCoords(x, z, newStructure);
    }

    //Params are worldcoords
    public Vector2Int getPositionInChunk(int x, int z)
    {
        int posX = x % Chunk.chunkSize;
        int posZ = z % Chunk.chunkSize;

        return new Vector2Int(posX, posZ);
    }
}
