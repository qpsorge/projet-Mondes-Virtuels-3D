using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{

    public int xSize = 1000;
    public int zSize = 1000;
    
    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;


    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        //StartCoroutine(CreateShape());
        CreateShape();
    }

    private void Update()
    {
        UpdateMesh();
    }

    //IEnumerator CreateShape()
    void CreateShape()
    {
        //vertex count = (xSize+1) * (zSize +1)

        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        float[,] map = VoronoiDemo.map;

        // VERTICES
        for (int z = 0; z <= zSize; z++)
        {
            for(int x=0; x <= xSize; x++)
            {
                //PERLIN NOISE FOR Y axis
                // map[x,z] = Mathf.PerlinNoise(x * .3f, z * .3f) * 2f;
                vertices[(zSize+1)*z+x] = new Vector3(x, map[x,z], z);
            }
        }



        // TRIANGLES
        triangles = new int[xSize*zSize*6];
        int vert = 0;
        int tris = 0;
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {

                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;

                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
                Debug.Log("x : "+x.ToString() + " z : "+ z.ToString()+ " tris : " + tris.ToString() + " vert : " + vert.ToString());
                //yield return new WaitForSeconds(0.01f);
            }
            vert++;
        }
            

        
    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
    }

    private void OnDrawGizmos()
    {
        /*
        if (vertices == null)
            return;
        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], .1f);
        }
        */
    }
}
