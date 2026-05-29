#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class InspectTiger
{
    static InspectTiger()
    {
        string assetPath = "Assets/戦車/Tiger_I.obj";
        GameObject TigerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (TigerPrefab == null)
        {
            File.WriteAllText("Assets/TigerStructure.txt", "Failed to load Tiger_I.obj at path: " + assetPath);
            return;
        }

        using (StreamWriter writer = new StreamWriter("Assets/TigerStructure.txt"))
        {
            writer.WriteLine("Tiger_I.obj Hierarchy Structure:");
            WriteHierarchy(TigerPrefab.transform, writer, "");
        }
    }

    static void WriteHierarchy(Transform t, StreamWriter writer, string indent)
    {
        string meshInfo = "";
        MeshFilter mf = t.GetComponent<MeshFilter>();
        if (mf != null && mf.sharedMesh != null)
        {
            meshInfo = $" [Mesh: {mf.sharedMesh.name}, Verts: {mf.sharedMesh.vertexCount}]";
        }
        writer.WriteLine($"{indent}- {t.name}{meshInfo}");
        for (int i = 0; i < t.childCount; i++)
        {
            WriteHierarchy(t.GetChild(i), writer, indent + "  ");
        }
    }
}
#endif
