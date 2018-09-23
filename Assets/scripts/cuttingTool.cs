using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class cuttingTool : MonoBehaviour
{

    public GameObject targetObject;
    public GameObject cuttingPlane;
    private Plane m_cuttingPlane;

    // Use this for initialization
    void Start()
    {
        cuttingPlane.transform.localPosition = new Vector3(0.0f, UnityEngine.Random.value * 4.0f - 1.0f, 0.0f);
        cuttingPlane.transform.localRotation = Quaternion.Euler(new Vector3(UnityEngine.Random.value * 30.0f, 0.0f, 0.0f));
    }

    // Update is called once per frame
    void Update()
    {
        DrawPlane(new Vector3(0.0f, 0.0f, 0.0f), m_cuttingPlane.normal);

    }

    public void DoCut()
    {

        print("cutting...");
        Mesh targetMesh = targetObject.GetComponent<MeshFilter>().mesh;
        Vector3[] verts = targetMesh.vertices;
        Vector3[] normals = targetMesh.normals;
        Vector4[] tangents = targetMesh.tangents;
        Vector2[] uv = targetMesh.uv;
        int[] triangles = targetMesh.triangles;
        bool[] side = new bool[verts.Length];
        Mesh planeMesh = cuttingPlane.GetComponent<MeshFilter>().mesh;
        Vector3 a = cuttingPlane.GetComponent<MeshFilter>().mesh.vertices[planeMesh.triangles[0]];
        Vector3 b = cuttingPlane.GetComponent<MeshFilter>().mesh.vertices[planeMesh.triangles[1]];
        Vector3 c = cuttingPlane.GetComponent<MeshFilter>().mesh.vertices[planeMesh.triangles[2]];
        m_cuttingPlane = new Plane(cuttingPlane.transform.TransformPoint(a), cuttingPlane.transform.TransformPoint(b), cuttingPlane.transform.TransformPoint(c));
        DrawPlane(new Vector3(0.0f, 0.0f, 0.0f), m_cuttingPlane.normal);
        List<Vector3> underMeshVerts = new List<Vector3>(verts);
        List<Vector3> underEdgeVerts = new List<Vector3>();
        List<Vector3> overMeshVerts = new List<Vector3>(verts);
        List<Vector3> overEdgeVerts = new List<Vector3>();
        List<Vector2> underUV = new List<Vector2>(uv);
        List<Vector2> overUV = new List<Vector2>(uv);
        List<Vector3> underNormals = new List<Vector3>(normals);
        List<Vector3> overNormals = new List<Vector3>(normals);
        List<Vector4> underTangents = new List<Vector4>(tangents);
        List<Vector4> overTangents = new List<Vector4>(tangents);
        List<int> underTriangles = new List<int>();
        List<int> overTriangles = new List<int>();
        float zeroPoint = targetObject.transform.InverseTransformPoint(m_cuttingPlane.ClosestPointOnPlane(targetObject.transform.TransformPoint(new Vector3(0.0f, 0.0f, 0.0f)))).z;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            if (m_cuttingPlane.GetSide(targetObject.transform.TransformPoint(verts[triangles[i]])) && m_cuttingPlane.GetSide(targetObject.transform.TransformPoint(verts[triangles[i + 1]])) && m_cuttingPlane.GetSide(targetObject.transform.TransformPoint(verts[triangles[i + 2]])))
            {
                overTriangles.Add(triangles[i]);
                overTriangles.Add(triangles[i + 1]);
                overTriangles.Add(triangles[i + 2]);
            }
            else if (!m_cuttingPlane.GetSide(targetObject.transform.TransformPoint(verts[triangles[i]])) && !m_cuttingPlane.GetSide(targetObject.transform.TransformPoint(verts[triangles[i + 1]])) && !m_cuttingPlane.GetSide(targetObject.transform.TransformPoint(verts[triangles[i + 2]])))
            {
                underTriangles.Add(triangles[i]);
                underTriangles.Add(triangles[i + 1]);
                underTriangles.Add(triangles[i + 2]);
            }
            else
            {
                List<int> aboveVertList = new List<int>();
                List<int> underVertList = new List<int>();
                for (int j = i; j < i + 3; j++)
                {
                    if (m_cuttingPlane.GetSide(targetObject.transform.TransformPoint(verts[triangles[j]])))
                    {
                        aboveVertList.Add(triangles[j]);
                    }
                    else
                    {
                        underVertList.Add(triangles[j]);
                    }
                }
                if (aboveVertList.Count == 1)
                {
                    Vector3 vert1 = verts[aboveVertList[0]];
                    Vector3 vert2_old = verts[underVertList[0]];
                    Vector3 vert3_old = verts[underVertList[1]];
                    Vector3 l_0 = targetObject.transform.TransformPoint(vert1);
                    Vector3 l = targetObject.transform.TransformVector(vert2_old - vert1);
                    Vector3 n = m_cuttingPlane.normal;
                    Vector3 p_0 = cuttingPlane.transform.position;
                    float denominator = Vector3.Dot(l, n);
                    float t = Vector3.Dot((p_0 - l_0), n) / denominator;
                    Vector3 vert2 = l_0 + l * t;
                    vert2 = targetObject.transform.InverseTransformPoint(vert2);
                    l = targetObject.transform.TransformVector(vert3_old - vert1);
                    denominator = Vector3.Dot(l, n);
                    t = Vector3.Dot((p_0 - l_0), n) / denominator;
                    Vector3 vert3 = l_0 + l * t;
                    vert3 = targetObject.transform.InverseTransformPoint(vert3);
                    overMeshVerts.Add(vert2);
                    overMeshVerts.Add(vert3);
                    overEdgeVerts.Add(vert2);
                    overEdgeVerts.Add(vert3);
                    int cont = 0;
                    for (int j = i; j < i + 3; j++)
                    {
                        if (m_cuttingPlane.GetSide(targetObject.transform.TransformPoint(verts[triangles[j]])))
                        {
                            overTriangles.Add(aboveVertList[0]);
                        }
                        else
                        {
                            overTriangles.Add(overMeshVerts.Count - 2 + cont);
                            overNormals.Add(normals[underVertList[cont]]);
                            overTangents.Add(tangents[underVertList[cont]]);
                            cont++;
                        }
                    }

                    vert1 = verts[underVertList[0]];
                    vert2 = verts[underVertList[1]];
                    vert3_old = verts[aboveVertList[0]];
                    l_0 = targetObject.transform.TransformPoint(vert1);
                    l = targetObject.transform.TransformVector(vert3_old - vert1);
                    n = m_cuttingPlane.normal;
                    p_0 = cuttingPlane.transform.position;
                    denominator = Vector3.Dot(l, n);
                    t = Vector3.Dot((p_0 - l_0), n) / denominator;
                    vert3 = l_0 + l * t;
                    vert3 = targetObject.transform.InverseTransformPoint(vert3);
                    l_0 = targetObject.transform.TransformPoint(vert2);
                    l = targetObject.transform.TransformVector(vert3_old - vert2);
                    denominator = Vector3.Dot(l, n);
                    t = Vector3.Dot((p_0 - l_0), n) / denominator;
                    Vector3 vert4 = l_0 + l * t;
                    vert4 = targetObject.transform.InverseTransformPoint(vert4);
                    underMeshVerts.Add(vert3);
                    underMeshVerts.Add(vert4);
                    underEdgeVerts.Add(vert3);
                    underEdgeVerts.Add(vert4);
                    underNormals.Add(normals[aboveVertList[0]]);
                    underTangents.Add(tangents[aboveVertList[0]]);
                    underNormals.Add(normals[aboveVertList[0]]);
                    underTangents.Add(tangents[aboveVertList[0]]);
                    cont = 0;
                    for (int j = i; j < i + 3; j++)
                    {
                        if (!m_cuttingPlane.GetSide(targetObject.transform.TransformPoint(verts[triangles[j]])))
                        {
                            underTriangles.Add(underVertList[cont]);
                            cont++;
                        }
                        else
                        {
                            underTriangles.Add(underMeshVerts.Count - 2);
                        }
                    }
                    cont = 0;
                    for (int j = i; j < i + 3; j++)
                    {
                        if (!m_cuttingPlane.GetSide(targetObject.transform.TransformPoint(verts[triangles[j]])))
                        {
                            underTriangles.Add(underMeshVerts.Count - 1 - cont);
                            cont++;
                        }
                        else
                        {
                            underTriangles.Add(underVertList[1]);
                        }
                    }
                }
                else
                {
                    Vector3 vert1 = verts[aboveVertList[0]];
                    Vector3 vert2 = verts[aboveVertList[1]];
                    Vector3 vert3_old = verts[underVertList[0]];
                    Vector3 l_0 = targetObject.transform.TransformPoint(vert1);
                    Vector3 l = targetObject.transform.TransformVector(vert3_old - vert1);
                    Vector3 n = m_cuttingPlane.normal;
                    Vector3 p_0 = cuttingPlane.transform.position;
                    float denominator = Vector3.Dot(l, n);
                    float t = Vector3.Dot((p_0 - l_0), n) / denominator;
                    Vector3 vert3 = l_0 + l * t;
                    vert3 = targetObject.transform.InverseTransformPoint(vert3);
                    l_0 = targetObject.transform.TransformPoint(vert2);
                    l = targetObject.transform.TransformVector(vert3_old - vert2);
                    denominator = Vector3.Dot(l, n);
                    t = Vector3.Dot((p_0 - l_0), n) / denominator;
                    Vector3 vert4 = l_0 + l * t;
                    vert4 = targetObject.transform.InverseTransformPoint(vert4);
                    overMeshVerts.Add(vert3);
                    overMeshVerts.Add(vert4);
                    overEdgeVerts.Add(vert3);
                    overEdgeVerts.Add(vert4);
                    overNormals.Add(normals[underVertList[0]]);
                    overTangents.Add(tangents[underVertList[0]]);
                    overNormals.Add(normals[underVertList[0]]);
                    overTangents.Add(tangents[underVertList[0]]);
                    int cont = 0;
                    for (int j = i; j < i + 3; j++)
                    {
                        if (m_cuttingPlane.GetSide(targetObject.transform.TransformPoint(verts[triangles[j]])))
                        {
                            overTriangles.Add(aboveVertList[cont]);
                            cont++;
                        }
                        else
                        {
                            overTriangles.Add(overMeshVerts.Count - 2);
                        }
                    }
                    cont = 0;
                    for (int j = i; j < i + 3; j++)
                    {
                        if (m_cuttingPlane.GetSide(targetObject.transform.TransformPoint(verts[triangles[j]])))
                        {
                            overTriangles.Add(overMeshVerts.Count - 1 - cont);
                            cont++;
                        }
                        else
                        {
                            overTriangles.Add(aboveVertList[1]);
                        }
                    }

                    vert1 = verts[underVertList[0]];
                    Vector3 vert2_old = verts[aboveVertList[0]];
                    vert3_old = verts[aboveVertList[1]];
                    l_0 = targetObject.transform.TransformPoint(vert1);
                    l = targetObject.transform.TransformVector(vert2_old - vert1);
                    n = m_cuttingPlane.normal;
                    p_0 = cuttingPlane.transform.position;
                    denominator = Vector3.Dot(l, n);
                    t = Vector3.Dot((p_0 - l_0), n) / denominator;
                    vert2 = l_0 + l * t;
                    vert2 = targetObject.transform.InverseTransformPoint(vert2);
                    l = targetObject.transform.TransformVector(vert3_old - vert1);
                    denominator = Vector3.Dot(l, n);
                    t = Vector3.Dot((p_0 - l_0), n) / denominator;
                    vert3 = l_0 + l * t;
                    vert3 = targetObject.transform.InverseTransformPoint(vert3);
                    underMeshVerts.Add(vert2);
                    underMeshVerts.Add(vert3);
                    underEdgeVerts.Add(vert2);
                    underEdgeVerts.Add(vert3);
                    cont = 0;
                    for (int j = i; j < i + 3; j++)
                    {
                        if (!m_cuttingPlane.GetSide(targetObject.transform.TransformPoint(verts[triangles[j]])))
                        {
                            underTriangles.Add(underVertList[0]);
                        }
                        else
                        {
                            underTriangles.Add(underMeshVerts.Count - 2 + cont);
                            underNormals.Add(normals[aboveVertList[cont]]);
                            underTangents.Add(tangents[aboveVertList[cont]]);
                            cont++;
                        }
                    }
                }
            }

        }

        Vector3 overEdgeMidPoint = new Vector3();
        for (int i = 0; i < overEdgeVerts.Count; i ++)
        {
            overEdgeMidPoint += overEdgeVerts[i];
        }
        overEdgeMidPoint = overEdgeMidPoint/ overEdgeVerts.Count;
        overMeshVerts.Add(overEdgeMidPoint);
        overNormals.Add(overNormals[overNormals.Count-1]);
        overTangents.Add(overTangents[overTangents.Count - 1]);


        Vector3 p0 = overMeshVerts[overMeshVerts.Count - 1];
        for (int i = 0; i < overEdgeVerts.Count; i +=2)
        {

            Vector3 p1 = overEdgeVerts[i];
            Vector3 p2 = overEdgeVerts[i + 1];
            overTriangles.Add(overMeshVerts.Count - 1);
            overTriangles.Add(overMeshVerts.IndexOf(overEdgeVerts[i]));
            overTriangles.Add(overMeshVerts.IndexOf(overEdgeVerts[i + 1]));
            overTriangles.Add(overMeshVerts.IndexOf(overEdgeVerts[i]));
            overTriangles.Add(overMeshVerts.Count - 1);
            overTriangles.Add(overMeshVerts.IndexOf(overEdgeVerts[i + 1]));

        }
        

        Vector3 underEdgeMidPoint = new Vector3();
        for (int i = 0; i < underEdgeVerts.Count; i++)
        {
            underEdgeMidPoint += overEdgeVerts[i];
        }
        underEdgeMidPoint = underEdgeMidPoint / underEdgeVerts.Count;
        underMeshVerts.Add(underEdgeMidPoint);
        underNormals.Add(underNormals[0]);
        underTangents.Add(underTangents[0]);


        for (int i = 0; i < underEdgeVerts.Count; i += 2)
        {
            underTriangles.Add(underMeshVerts.IndexOf(underEdgeVerts[i]));
            underTriangles.Add(underMeshVerts.Count - 1);
            underTriangles.Add(underMeshVerts.IndexOf(underEdgeVerts[i + 1]));
        }

        for (int i = 0; i < overEdgeVerts.Count; i += 2)
        {
            underTriangles.Add(underMeshVerts.IndexOf(underEdgeVerts[i]));
            underTriangles.Add(underMeshVerts.IndexOf(underEdgeVerts[i + 1]));
            underTriangles.Add(underMeshVerts.Count - 1);
        }


        Mesh underMesh = new Mesh();
        Mesh overMesh = new Mesh();
        underMesh.vertices = underMeshVerts.ToArray();
        underMesh.triangles = underTriangles.ToArray();
        underMesh.tangents = underTangents.ToArray();
        underMesh.normals = underNormals.ToArray();
        underMesh.RecalculateNormals();
        underMesh.RecalculateTangents();
        underMesh.name = "under";
        GameObject underObject = Instantiate(targetObject, targetObject.transform.parent) as GameObject;
        underObject.GetComponent<MeshFilter>().mesh = underMesh;
        overMesh.vertices = overMeshVerts.ToArray();
        overMesh.triangles = overTriangles.ToArray();
        overMesh.tangents = overTangents.ToArray();
        overMesh.normals = overNormals.ToArray();
        overMesh.RecalculateNormals();
        overMesh.RecalculateTangents();
        overMesh.name = "over";
        GameObject overObject = Instantiate(targetObject, targetObject.transform.parent) as GameObject;
        overObject.GetComponent<MeshFilter>().mesh = overMesh;
        targetObject.SetActive(false);
    }


    public void DrawPlane(Vector3 position, Vector3 normal)
    {

        Vector3 v3;

        if (normal.normalized != Vector3.forward)
            v3 = Vector3.Cross(normal, Vector3.forward).normalized * normal.magnitude;
        else
            v3 = Vector3.Cross(normal, Vector3.up).normalized * normal.magnitude; ;

        var corner0 = position + v3;
        var corner2 = position - v3;
        var q = Quaternion.AngleAxis(90.0f, normal);
        v3 = q * v3;
        var corner1 = position + v3;
        var corner3 = position - v3;

        Debug.DrawLine(corner0, corner2, Color.green);
        Debug.DrawLine(corner1, corner3, Color.green);
        Debug.DrawLine(corner0, corner1, Color.green);
        Debug.DrawLine(corner1, corner2, Color.green);
        Debug.DrawLine(corner2, corner3, Color.green);
        Debug.DrawLine(corner3, corner0, Color.green);
        Debug.DrawRay(position, normal, Color.red);
    }
}
