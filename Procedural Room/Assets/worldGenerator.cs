using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class worldGenerator : MonoBehaviour
{
    public int RoomMin = 4;
    public int RoomMax = 8;
    public int mapSizeX = 20;
    public int mapSizeY = 20;
    public int debug = 1;
    public int minRoomWidth = 1;
    public int maxTries = 5;
    public int randomSpawnDistance = 4;
    public int maxHiddenCorner = 1;
    public int generateInsideTreshold = 10;
    public float unitSize = 0.2f;
    private int[,] map;
    private Vector3[] vertices;
    private char[] vertexIsWall;
    private StreamWriter writer;

    //private Vector3[] vertices;
    //private Mesh mesh;

    // Start is called before the first frame update
    void Start()
    {
        InitializeMap();
        ClearFile();
        int iterations = GenerateMap(0, 1);
        if (debug >= 1) print("Iteratii: " + iterations);

        GenerateFloorMesh();

    }

    void GenerateFloorMesh()
    {
        //WaitForSeconds wait = new WaitForSeconds(0.05f);
        GameObject floor;
        Mesh mesh;
        floor = new GameObject();
        floor.AddComponent<MeshFilter>();
        floor.AddComponent<MeshRenderer>();
        floor.GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        int k = 0;
        for (int i = 0; i <= mapSizeX; i++)
        {
            for (int j = 0; j <= mapSizeY; j++)
            {
                if (map[i, j] == -1 || map[i, j] == 1)
                {
                    k++;
                }
            }
        }
        if (debug >= 1) print(k);
        vertices = new Vector3[k];
        vertexIsWall = new char[k];
        k = 0;
        int quad = 0;
        for (int i = 0; i <= mapSizeX; i++)
        {
            for (int j = 0; j <= mapSizeY; j++)
            {
                if (map[i, j] == -1 || map[i, j] == 1)
                {
                    if (i < mapSizeX && j < mapSizeY)
                        if ((map[i, j + 1] == 1 || map[i, j + 1] == -1) && (map[i + 1, j] == 1 || map[i + 1, j] == -1) && (map[i + 1, j + 1] == 1 || map[i + 1, j + 1] == -1))
                            quad++;
                    vertices[k] = new Vector3(i * unitSize, 0, j * unitSize);
                    vertexIsWall[k] = (map[i, j] == -1) ? (char)1 : (char)0;
                    k++;
                }
            }
        }
        if (debug >= 2) print(quad);
        mesh.vertices = vertices;
        print(mesh.vertices.Length);
        int[] triangles = new int[quad * 6];
        int maxVert = 0;
        for (int ti = 0, vi = 0, x = 0; x < mapSizeX; x++, vi++)
        {
            bool nullRow = true;
            for (int y = 0; y < mapSizeY; y++)
            {
                if ((map[x, y] == 1 || map[x, y] == -1))
                {
                    if ((map[x, y + 1] == 1 || map[x, y + 1] == -1) && (map[x + 1, y] == 1 || map[x + 1, y] == -1) && (map[x + 1, y + 1] == 1 || map[x + 1, y + 1] == -1))
                    {
                        if (debug >= 3) print("generate tri pair at : " + x + " " + y + " " + (x + 1) + " " + (y + 1));
                        int leap = 0;
                        for (int i = x, j = y; i <= x + 1; i++)
                        {
                            for (; j <= mapSizeY; j++)
                            {
                                if (map[i, j] == 1 || map[i, j] == -1)
                                {

                                    if (i == x + 1 && j == y)
                                        break;
                                    leap++;
                                }
                            }
                            j = 0;
                        }
                        if (debug >= 3) print(leap);
                        if (debug >= 3) print(vi + " " + (vi + 1) + " " + (vi + leap));
                        triangles[ti] = vi;
                        triangles[ti + 1] = vi + 1;
                        triangles[ti + 2] = vi + leap;
                        triangles[ti + 3] = vi + leap;
                        triangles[ti + 4] = vi + 1;
                        triangles[ti + 5] = vi + leap + 1;
                        ti += 6;

                        if (debug >= 3) print("generated");
                        maxVert = vi + leap + 1;
                        nullRow = false;
                    }
                    for (int i = y + 1; i <= mapSizeY; i++)
                        if (map[x, i] == -1 || map[x, i] == 1)
                        {
                            vi++;
                            break;
                        }
                }
            }
            if (nullRow) vi--;
            //mesh.triangles = triangles;
            //yield return wait;
        }
        print(maxVert);
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }


    void InitializeMap()
    {
        map = new int[mapSizeX + 1, mapSizeY + 1];
        for (int i = 0; i <= mapSizeX; i++)
            for (int j = 0; j <= mapSizeY; j++)
                map[i, j] = 0;
    }
    void ClearFile()
    {
        writer = new StreamWriter("D:\\weMeshUP\\OneDrive\\Unity\\Procedural Room\\Procedural Room\\Assets\\map.txt", false);
        writer.Close();
    }
    void MapIterToString(bool empty)
    {
        writer = new StreamWriter("D:\\weMeshUP\\OneDrive\\Unity\\Procedural Room\\Procedural Room\\Assets\\map.txt", true);
        string mapString = "";
        for (int j = mapSizeY; j >= 0; j--)
        {
            for (int i = 0; i <= mapSizeX; i++)
            {
                if (map[i, j] == -1) mapString += "# ";
                else if (map[i, j] == 0) mapString += "0 ";
                else
                {
                    if (empty)
                        mapString += (char)(map[i, j] + 64) + " ";
                    else
                        mapString += "  ";
                }
            }
            mapString += "\n";
        }
        mapString += "\n";
        writer.Write(mapString);
        writer.Close();
    }

    int GenerateMap(int max, int iter)
    {
        //https://www.youtube.com/watch?time_continue=91&v=gmuHI_wsOgI&feature=emb_logo
        int x = Random.Range(RoomMin, RoomMax);
        int y = Random.Range(RoomMin, RoomMax);
        if (debug >= 2) print("X:" + x + " Y:" + y);
        bool space = false;
        for (int i = Random.Range(0, randomSpawnDistance); i <= mapSizeX; i++)
        {
            if (!space)
            {
                for (int j = Random.Range(0, randomSpawnDistance); j <= mapSizeY; j++)
                {
                    if (!space)
                    {
                        if ((map[i, j] <= 0 || Random.Range(0, 100) < generateInsideTreshold) && i <= mapSizeX - x && j <= mapSizeY - y)
                        {
                            if (CheckForSpace(i, j, x, y))
                            {
                                if (debug >= 2) print("found empty at: " + i + " " + j);
                                if (i + x <= mapSizeX && j + y <= mapSizeY)
                                {
                                    int ok = -1;
                                    if (map[i + x, j] <= 0)
                                        ok += -1;
                                    else
                                        ok += 1;
                                    if (map[i, j + y] <= 0)
                                        ok += -1;
                                    else
                                        ok += 1;
                                    if (map[i + x, j + y] <= 0)
                                        ok += -1;
                                    else
                                        ok += 1;
                                    if (debug >= 2) print(ok);
                                    if (ok <= 0)
                                    {
                                        Fill(i, j, x, y, iter);
                                        space = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        if (space && iter <= 100)
            return GenerateMap(max + 1, iter + 1);
        else if (max < maxTries && iter <= 100)
            return GenerateMap(max + 1, iter);
        return iter;
    }

    bool CheckForSpace(int posX, int posY, int x, int y)
    {
        int ok = 0;
        for (int k = 1; k <= minRoomWidth; k++)
        {
            if (map[posX + k, posY + k] != 0)
            {
                ok++;
                break;
            }
        }
        for (int k = 1; k <= minRoomWidth; k++)
        {
            if (map[posX + x - k, posY + k] != 0)
            {
                ok++;
                break;
            }
        }
        for (int k = 1; k <= minRoomWidth; k++)
        {
            if (map[posX + k, posY + y - k] != 0)
            {
                ok++;
                break;
            }
        }
        for (int k = 1; k <= minRoomWidth; k++)
        {
            if (map[posX + x - k, posY + y - k] != 0)
            {
                ok++;
                break;
            }
        }
        if (ok <= maxHiddenCorner)
            return true;
        else
            return false;
    }

    void Fill(int coordX, int coordY, int sizeX, int sizeY, int roomNo)
    {
        for (int i = coordX; i <= coordX + sizeX; i++)
            for (int j = coordY; j <= coordY + sizeY; j++)
            {
                if (debug >= 2) print("check " + i + " " + j + "=" + map[i, j]);
                if (map[i, j] == 0)
                {
                    if (debug >= 2) print("filling " + i + " " + j);
                    if (i == coordX || i == coordX + sizeX || j == coordY || j == coordY + sizeY)
                    {
                        map[i, j] = -1;
                    }
                    else
                        map[i, j] = roomNo;
                }
            }
        if (debug >= 1) MapIterToString(false);
    }

    /*private void GenerateTestMesh()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Grid";
        int xSize = RoomMin;
        int ySize = RoomMax;

        vertices = new Vector3[(xSize + 1) * (ySize + 1)];
        for (int i = 0, y = 0; y <= ySize; y++)
        {
            for (int x = 0; x <= xSize; x++, i++)
            {
                vertices[i] = new Vector3(x, y);
            }
        }
        mesh.vertices = vertices;

        int[] triangles = new int[xSize * ySize * 6];
        for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++)
        {
            for (int x = 0; x < xSize; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
                triangles[ti + 5] = vi + xSize + 2;
            }
        }
        mesh.triangles = triangles;

        for (int i = (RoomMin+1)*3, y = 3; y <= 5; y++)
        {
            for (int x = 0; x <= xSize; x++, i++)
            {
                vertices[i] = new Vector3(x, y, 1);
            }
        }
        mesh.vertices = vertices;
    }

    
    void PrintMap()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Grid";

        vertices = new Vector3[(mapSizeX + 1) * (mapSizeY + 1)];
        for (int i = 0, y = 0; y <= mapSizeX; y++)
        {
            for (int x = 0; x <= mapSizeY; x++)
            {
                if (map[x, y] != 0) { 
                    vertices[i] = new Vector3((float)-mapSizeX / 2 + x, 0, (float)-mapSizeY / 2 + y);
                    i++;
                }
            }
        }
        mesh.vertices = vertices;
    }
            

    void GenerateMapVector(int x, int y)
    {
        for (int i = 0; i <= x; i++)
            for (int j = 0; j <= y; j++)
            {
                map[mapSizeX / 2 - x/2 + i,mapSizeY / 2 - y/2 + j] = 1;
            }
        PrintMap();
        
    }
    void GenerateRoom(int xSize,int ySize)
    {

        //https://catlikecoding.com/unity/tutorials/procedural-grid/
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Grid";

        vertices = new Vector3[(xSize+1)*(ySize+1)];
        Vector2[] uv = new Vector2[vertices.Length];
        Vector4[] tangents = new Vector4[vertices.Length];
        Vector4 tangent = new Vector4(0f, 1f, 0f, -1f);

        for (int i = 0, y = 0; y <= ySize; y++)
        {
            for (int x = 0; x <= xSize; x++, i++)
            {
                vertices[i] = new Vector3((float)-xSize/2 + x, 0,(float) -ySize/2 + y);
                uv[i] = new Vector2(x / xSize, y / ySize);
                tangents[i] = tangent;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.tangents = tangents;

        int[] triangles = new int[xSize * ySize * 6];
        for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++)
        {
            for (int x = 0; x < xSize; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
                triangles[ti + 5] = vi + xSize + 2;
            }
        }
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }*/
    private void OnDrawGizmos()
    {
        if (vertices == null)
        {
            return;
        }
        Gizmos.color = Color.black;
        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertexIsWall[i] == 1)
                Gizmos.DrawSphere(vertices[i], 0.05f);
        }
    }
}
