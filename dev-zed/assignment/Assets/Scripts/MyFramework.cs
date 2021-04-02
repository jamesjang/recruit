using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class MyFramework : MonoBehaviour
{
    private string jsonPath;
    [SerializeField] private Texture2D textureMap;

    private void Awake()
    {
        jsonPath = Path.Combine(Application.dataPath, Constants.JSON_DATA_PATH);
    }

    void Start()
    {
        ParseJson(jsonPath);
    }

    void ParseJson(string path)
    {
        string jsonText = File.ReadAllText(path);

        Response dataInJson = JsonUtility.FromJson<Response>(jsonText);

        if (dataInJson.success && dataInJson.code == Constants.SUCCESS_CODE)
        {
            if (dataInJson.data.Count > 0)
            {
                CreateBuildingFromData(dataInJson.data.ToArray());
            }
        }
    }

    void CreateBuildingFromData(BuildingData[] data)
    {
        foreach (BuildingData building in data)
        {
            GameObject go = new GameObject(building.meta.동);

            // create material with standard urp shader
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            Material material = new Material(shader) { mainTexture = textureMap };

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            for (int i = 0; i < building.roomtypes.Length; i++)
            {
                string[] verticesData = building.roomtypes[i].coordinatesBase64s;

                foreach (string vertex in verticesData)
                {
                    // Convert Base64 coordinates to float.
                    byte[] bytes = System.Convert.FromBase64String(vertex);
                    float[] floats = new float[bytes.Length / 4];

                    // BlockCopy byte array to float array
                    System.Buffer.BlockCopy(bytes, 0, floats, 0, bytes.Length);

                    // Convert list of floats to array of vertices
                    for (int k = 0; k < floats.Length; k += 3)
                    {
                        vertices.Add(new Vector3(floats[k], floats[k + 2], floats[k + 1]));
                    }

                    // Get triangles for mesh
                    for (int l = 0; l < vertices.Count; l += 3)
                    {
                        triangles.Add(l);
                        triangles.Add(l + 1);
                        triangles.Add(l + 2);
                    }
                }
            }

            GenerateMesh(go, vertices.ToArray(), triangles.ToArray(), material);
        }
    }


    private void GenerateMesh(GameObject go, Vector3[] vertices, int[] triangles, Material material)
    {
        Mesh mesh = new Mesh { vertices = vertices, triangles = triangles };
        MeshFilter meshFilter = go.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
        meshFilter.mesh = mesh;
        meshRenderer.material = material;

        float buildingHeight = vertices.Max(v => v.y) + vertices.Min(v => v.y);

        material.SetTextureScale("_BaseMap", new Vector2(1.0f, (int)(buildingHeight / 3.0f)));
        mesh.RecalculateNormals();

        // Generate UVs
        GenerateUV(mesh);
    }

    private void GenerateUV(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        Vector2[] uvs = new Vector2[vertices.Length];

        // Height should always be scaled to entire building because of small quads

        for (int i = 0; i < vertices.Length; i += 6)
        {
            Vector3 normal = normals[i];
            float angle = Vector3.Angle(normal, Vector3.forward) + 180.0f;

            // Get quad vertices
            Vector3[] quadVertices = vertices.Skip(i).Take(6).ToArray();

            // Check if quad is part of a bigger building
            bool smallQuad = quadVertices.Max(value => value.y) > vertices.Max(value => value.y);

            // get min max vertices of quad
            MinMaxValue minMaxX = new MinMaxValue
            {
                Min = quadVertices.Min(value => value.x),
                Max = quadVertices.Max(value => value.x)
            };

            MinMaxValue minMaxY = new MinMaxValue
            {
                Min = quadVertices.Min(value => value.y),
                Max = smallQuad ? quadVertices.Max(value => value.y) : vertices.Max(value => value.y)
            };

            MinMaxValue minMaxZ = new MinMaxValue
            {
                Min = quadVertices.Min(value => value.z),
                Max = quadVertices.Max(value => value.z)
            };

            // How much of the texture will be mapped and the starting offset
            float textureAmount, textureOffset;

            // Top & Bottom
            if (normal == Vector3.up || normal == Vector3.down)
            {
                textureAmount = 0.25f;
                textureOffset = 0.75f;

                for (int j = 0; j < quadVertices.Length; j++)
                {
                    uvs[i + j] = new Vector2
                    {
                        x = Mathf.InverseLerp(minMaxX.Min, minMaxX.Max, quadVertices[j].x) * textureAmount + textureOffset,
                        y = Mathf.InverseLerp(minMaxZ.Min, minMaxZ.Max, quadVertices[j].z)
                    };
                }
            }
            // Front & Back
            else if (180.0f <= angle && angle <= 220.0f)
            {
                textureAmount = 0.5f;
                textureOffset = 0.0f;

                for (int j = 0; j < quadVertices.Length; j++)
                {
                    uvs[i + j] = new Vector2
                    {
                        x = Mathf.InverseLerp(minMaxX.Min, minMaxX.Max, quadVertices[j].x) * textureAmount + textureOffset,
                        y = Mathf.InverseLerp(minMaxY.Min, minMaxY.Max, quadVertices[j].y)
                    };
                }
            }
            // Sides
            else
            {
                textureAmount = 0.25f;
                textureOffset = 0.5f;

                for (int j = 0; j < quadVertices.Length; j++)
                {
                    uvs[i + j] = new Vector2
                    {
                        x = Mathf.InverseLerp(minMaxZ.Min, minMaxZ.Max, quadVertices[j].z) * textureAmount + textureOffset,
                        y = Mathf.InverseLerp(minMaxY.Min, minMaxY.Max, quadVertices[j].y)
                    };
                }
            }
        }

        mesh.uv = uvs;
    }

    private struct MinMaxValue
    {
        public float Min;
        public float Max;
    }
}