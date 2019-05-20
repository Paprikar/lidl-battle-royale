using UnityEngine;
using UnityEditor;
using System.Collections;

// an Editor method to create a cone primitive (so far no end caps)
// the top center is placed at (0/0/0)
// the bottom center is placed at (0/0/length)
// if either one of the radii is 0, the result will be a cone, otherwise a truncated cone
// note you will get inevitable breaks in the smooth shading at cone tips
// note the resulting mesh will be created as an asset in Assets/Editor
// Author: Wolfram Kresse
public class Cone : ScriptableWizard
{

    public int subdivisions = 10;
    public float radius = 1f;
    public float height = 1f;
    public bool addCollider = false;

    [MenuItem("GameObject/Create Other/Cone2")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard("Create Cone", typeof(Cone));
    }

    void OnWizardCreate()
    {
        GameObject newCone = new GameObject("Cone");
        string meshName = newCone.name + subdivisions + "v" + radius + "r" + height + "l";
        string meshPrefabPath = "Assets/Editor/" + meshName + ".asset";
        Mesh mesh = (Mesh)AssetDatabase.LoadAssetAtPath(meshPrefabPath, typeof(Mesh));
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = meshName;

            Vector3[] vertices = new Vector3[subdivisions + 2];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[(subdivisions * 2) * 3];

            vertices[0] = Vector3.zero;
            uv[0] = new Vector2(0.5f, 0f);
            for(int i = 0, n = subdivisions - 1; i < subdivisions; i++) {
                float ratio = (float)i / n;
                float r = ratio * (Mathf.PI * 2f);
                float x = Mathf.Cos(r) * radius;
                float z = Mathf.Sin(r) * radius;

                vertices[i + 1] = new Vector3(x, 0f, z);
                uv[i + 1] = new Vector2(ratio, 0f);
            }
            vertices[subdivisions + 1] = new Vector3(0f, height, 0f);
            uv[subdivisions + 1] = new Vector2(0.5f, 1f);


            // construct bottom
            for(int i = 0, n = subdivisions - 1; i < n; i++) {
                int offset = i * 3;
                triangles[offset] = 0;
                triangles[offset + 1] = i + 1;
                triangles[offset + 2] = i + 2;
            }


            // construct sides
            int bottomOffset = subdivisions * 3;
            for(int i = 0, n = subdivisions - 1; i < n; i++) {
                int offset = i * 3 + bottomOffset;
                triangles[offset] = i + 1;
                triangles[offset + 1] = subdivisions + 1;
                triangles[offset + 2] = i + 2;
            }

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            AssetDatabase.CreateAsset(mesh, meshPrefabPath);
            AssetDatabase.SaveAssets();
        }


        MeshFilter mf = newCone.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        newCone.AddComponent<MeshRenderer>();

        if (addCollider)
        {
            MeshCollider mc = newCone.AddComponent<MeshCollider>();
            mc.sharedMesh = mf.sharedMesh;
        }

        Selection.activeObject = newCone;
    }
}
