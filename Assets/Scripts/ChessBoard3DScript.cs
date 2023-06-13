using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessBoard3DScript : MonoBehaviour
{

    private const int TILE_COUNT_Y = 8;
    private const int TILE_COUNT_X = 8;
    private const float tileSize = 1.0f;
    private GameObject[,] tiles;

    public Material tileBlackMat, tileWhiteMat, HoverMaterial;

    Camera cam;
    public Vector2Int currentHover;


    void Awake()
    {
        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);

    }

    void Update()
    {

        if (cam == null)
        { cam = Camera.main; return; }


        RaycastHit info;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 10000, LayerMask.GetMask("Tile", "Hover")))
        {

            Vector2Int hitTilePos = LoockupTileIndex(info.transform.gameObject);
            if (currentHover == -Vector2Int.one)
            {

                currentHover = hitTilePos;
                tiles[hitTilePos.x, hitTilePos.y].layer = LayerMask.NameToLayer("Hover");
                tiles[hitTilePos.x, hitTilePos.y].GetComponent<MeshRenderer>().material = HoverMaterial;

            }

            if (currentHover != hitTilePos)
            {
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                tiles[currentHover.x, currentHover.y].GetComponent<MeshRenderer>().material=(currentHover.x + currentHover.y) % 2 == 0?tileWhiteMat:tileBlackMat;
                currentHover = hitTilePos;
                tiles[hitTilePos.x, hitTilePos.y].layer = LayerMask.NameToLayer("Hover");
                tiles[hitTilePos.x, hitTilePos.y].GetComponent<MeshRenderer>().material=HoverMaterial;


            }

        }
        else
        {
            if (currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                tiles[currentHover.x, currentHover.y].GetComponent<MeshRenderer>().material=(currentHover.x + currentHover.y) % 2 == 0?tileWhiteMat:tileBlackMat;
                currentHover = -Vector2Int.one;

            }
        }
    }

    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        tiles = new GameObject[tileCountX, tileCountY];

        for (int x = 0; x < tileCountX; x++)
            for (int y = 0; y < tileCountY; y++)
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);





    }
    private Vector2Int LoockupTileIndex(GameObject HitObject)
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (tiles[x, y] == HitObject)
                    return new Vector2Int(x, y);

        return -Vector2Int.one;
    }

    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tile = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tile.transform.parent = transform;
        Mesh mesh = new Mesh();

        tile.AddComponent<MeshFilter>().mesh = mesh;
        tile.AddComponent<MeshRenderer>();


        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, 0, y * tileSize);
        vertices[1] = new Vector3((x) * tileSize, 0, (y + 1) * tileSize);
        vertices[2] = new Vector3((x + 1) * tileSize, 0, y * tileSize);
        vertices[3] = new Vector3((x + 1) * tileSize, 0, (y + 1) * tileSize);

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };
        tile.gameObject.layer = LayerMask.NameToLayer("Tile");
        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateBounds();
        tile.AddComponent<BoxCollider>();
        bool isWhiteSquare = false;
        if ((x + y) % 2 == 0)
            isWhiteSquare = true;
        tile.GetComponent<MeshRenderer>().material =
       isWhiteSquare ? tileWhiteMat : tileBlackMat;

        return tile;
    }
}
