using UnityEngine;

/// <summary>
/// 円状の薄い壁（両面描画のチューブ状メッシュ）を動的に生成するスクリプトです。
/// 地形のでこぼこに関わらずゾーンを視認しやすくします。
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ZoneVisualWall : MonoBehaviour
{
    public float radius = 4f;
    public float height = 8f;
    public int segments = 32;

    /// <summary>
    /// 起動時にメッシュを生成します。
    /// </summary>
    void Start()
    {
        GenerateMesh();
    }

    /// <summary>
    /// 動的にチューブ状のメッシュを生成し、MeshFilter に設定します。
    /// </summary>
    void GenerateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "ZoneWallMesh";

        // 頂点数と配列の初期化。両面描画のため、1セグメントにつき外側・内側それぞれに頂点を用意します
        int numVertices = (segments + 1) * 4;
        Vector3[] vertices = new Vector3[numVertices];
        Vector2[] uv = new Vector2[numVertices];
        int[] triangles = new int[segments * 12];

        float angleStep = Mathf.PI * 2f / segments;

        int v = 0;
        int t = 0;

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * angleStep;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            // 地面にめり込んでもいいように、下方向にも少し伸ばす
            Vector3 bottom = new Vector3(cos * radius, -2f, sin * radius);
            Vector3 top = new Vector3(cos * radius, height, sin * radius);

            // 外側の頂点
            vertices[v] = bottom;
            vertices[v + 1] = top;
            
            // 内側の頂点
            vertices[v + 2] = bottom;
            vertices[v + 3] = top;

            uv[v] = new Vector2((float)i / segments, 0);
            uv[v + 1] = new Vector2((float)i / segments, 1);
            uv[v + 2] = new Vector2((float)i / segments, 0);
            uv[v + 3] = new Vector2((float)i / segments, 1);

            if (i < segments)
            {
                // 外側のポリゴン（三角形）
                triangles[t] = v;
                triangles[t + 1] = v + 1;
                triangles[t + 2] = v + 4;

                triangles[t + 3] = v + 1;
                triangles[t + 4] = v + 5;
                triangles[t + 5] = v + 4;

                // 内側のポリゴン（逆回りにして内側を向かせる）
                triangles[t + 6] = v + 2;
                triangles[t + 7] = v + 6;
                triangles[t + 8] = v + 3;

                triangles[t + 9] = v + 3;
                triangles[t + 10] = v + 6;
                triangles[t + 11] = v + 7;

                t += 12;
            }

            v += 4;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
    }
}
