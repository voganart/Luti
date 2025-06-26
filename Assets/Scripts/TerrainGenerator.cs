using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class TerrainGenerator : MonoBehaviour
{
    public int width = 512;
    public int height = 512;
    public float scale = 20;

    void Start()
    {
        Terrain terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrain(terrain.terrainData);
    }

    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, 50, height);
        terrainData.SetHeights(0, 0, GenerateHeights());
        return terrainData;
    }

    float[,] GenerateHeights()
    {
        float[,] heights = new float[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float xCoord = (float)x / width * scale;
                float yCoord = (float)y / height * scale;
                heights[x, y] = Mathf.PerlinNoise(xCoord, yCoord);
            }
        }
        return heights;
    }
}
