using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System;
using UnityEngine;

public class TerrainLoad : MonoBehaviour
{
    [SerializeField] Terrain Terrain;
    struct TerrainHeightData
    {
        public string file_title;
        public string data_format;
        public string map_projection;
        public string ellipsoid;
        public float left_map_x;
        public float lower_map_y;
        public float right_map_x;
        public float upper_map_y;
        public int numder_of_rows;
        public int number_of_columns;
        public string elev_m_unit;
        public float elev_m_minimum;
        public float elev_m_maximum;
        public int elev_m_missing_flag;

        public float[,] normalize_data;
        public float[,] height_map;

        private const float EarthRadius = 6378.0e3f;
        private const float m_per_deg = Mathf.PI * EarthRadius / 180.0f;

        public float x_range
        {
            get
            {
                return (right_map_x - left_map_x) * m_per_deg;
            }
        }

        public float y_range
        {
            get
            {
                return elev_m_maximum - elev_m_minimum;
            }
        }

        public float z_range
        {
            get
            {
                return (upper_map_y - lower_map_y) * m_per_deg;
            }
        }
    }

    TerrainHeightData terrainData;

    string filePath = "D:\\files\\ITMO\\VKR\\filepath\\testTerrain.hdr";
    int resolution = 513;

    private void Start()
    {
        if (!load(filePath, resolution)) Debug.Log("Terrain is not load");

        Terrain.terrainData.size = new Vector3(900, 25, 900);
        Terrain.terrainData.heightmapResolution = resolution;
        Terrain.terrainData.SetHeights(0, 0, heightMap);
    }

    public float[,] heightMap
    {
        get
        {
            return terrainData.height_map;
        }
    }

    public bool load(string path, int resolution)
    {
        try
        {
            // Read all strings from header file
            string[] header_data = File.ReadAllLines(path);

            // Parse all strings
            Dictionary<string, string> header_params = new Dictionary<string, string>();

            for (int i = 0; i < header_data.Length; i++)
            {
                string[] param = header_data[i].Split('=');
                header_params.Add(EraseSpaces(param[0]), EraseSpaces(param[1]));
            }

            if (!ParseHeader(header_params, ref terrainData))
                return false;

            // Allocate memory 
            terrainData.normalize_data = new float[terrainData.numder_of_rows,
                                                    terrainData.number_of_columns];

            // Get directory of binary data
            string dir = Path.GetDirectoryName(path);
            string data_path = Path.Combine(dir, terrainData.file_title + ".bin");

            // Open binary data file
            BinaryReader reader = new BinaryReader(File.Open(data_path, FileMode.Open));

            // Data reading
            for (int i = 0; i < terrainData.numder_of_rows; i++)
            {
                for (int j = 0; j < terrainData.number_of_columns; j++)
                {
                    float height = reader.ReadSingle();
                    float level = (height - terrainData.elev_m_minimum) / terrainData.y_range;

                    if (level > 1.0f)
                        level = 1.0f;

                    if (level < 0.0f)
                        level = 0.0f;

                    terrainData.normalize_data[i, j] = level;
                }
            }

            // Height map generation
            terrainData.height_map = new float[resolution, resolution];

            float dx = terrainData.x_range / resolution - 1;
            float dz = terrainData.z_range / resolution - 1;

            for (int i = 0; i < resolution; i++)
            {
                for (int j = 0; j < resolution; j++)
                {
                    terrainData.height_map[i, j] = getHeight(i * dx, j * dz);
                }
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private bool ParseHeader(Dictionary<string, string> header_data,
                         ref TerrainHeightData data)
    {
        try
        {
            data.file_title = header_data["file_title"];
        }
        catch (Exception)
        {
            return false;
        }

        try
        {
            data.data_format = header_data["data_format"];
        }
        catch (Exception)
        {

        }

        try
        {
            data.map_projection = header_data["map_projection"];
        }
        catch (Exception)
        {

        }

        try
        {
            data.ellipsoid = header_data["ellipsoid"];
        }
        catch (Exception)
        {

        }

        try
        {
            data.left_map_x = float.Parse(header_data["left_map_x"], CultureInfo.InvariantCulture);
            data.lower_map_y = float.Parse(header_data["lower_map_y"], CultureInfo.InvariantCulture);
            data.right_map_x = float.Parse(header_data["right_map_x"], CultureInfo.InvariantCulture);
            data.upper_map_y = float.Parse(header_data["upper_map_y"], CultureInfo.InvariantCulture);
            data.numder_of_rows = int.Parse(header_data["number_of_rows"]);
            data.number_of_columns = int.Parse(header_data["number_of_columns"]);
            data.elev_m_unit = header_data["elev_m_unit"];
            data.elev_m_minimum = float.Parse(header_data["elev_m_minimum"], CultureInfo.InvariantCulture);
            data.elev_m_maximum = float.Parse(header_data["elev_m_maximum"], CultureInfo.InvariantCulture);
            data.elev_m_missing_flag = int.Parse(header_data["elev_m_missing_flag"]);
        }
        catch (Exception)
        {
            return false;
        }

        return true;

    }

    private string EraseSpaces(string str)
    {
        string tmp = "";

        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] != ' ')
                tmp += str[i];
        }

        return tmp;
    }

    float getHeight(float x, float z)
    {
        float height = 0.0f;

        // Coordinate steps calculation
        float dx = terrainData.x_range / (terrainData.numder_of_rows - 1);
        float dz = terrainData.z_range / (terrainData.number_of_columns - 1);

        // Get base point indexes
        int i = (int)(x / dx);
        int j = (int)(z / dz);

        // Check range of indexes
        if ((i >= terrainData.numder_of_rows) || (j >= terrainData.number_of_columns))
            return 0.0f;

        // Calculate partial derivatives in point (i, j)
        float dydx = (terrainData.normalize_data[i + 1, j] - terrainData.normalize_data[i, j]) / dx;
        float dydz = (terrainData.normalize_data[i, j + 1] - terrainData.normalize_data[i, j]) / dz;

        // Linear height approximation
        height = terrainData.normalize_data[i, j] + dydx * (x - i * dx) + dydz * (z - j * dz);

        return height;
    }
}
