using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ChessBoard : MonoBehaviour
{
    // Board setup
    private const int TileCountX = 8;
    private const int TileCountY = 8;
    private const float TileSize = 1.0f;
    private GameObject[,] tiles;

    public Sprite tileSprite;

    private ChessPiece currentlyDraggingPiece;
    private ChessPiece[,] chessPieces = new ChessPiece[32, 32];
    public GameObject[] blackPieces;
    public GameObject[] whitePieces;
    public Color tileBlackColor;
    public Color tileWhiteColor;
    public Color hoverColor;
    private Vector2Int previousPiecePosition;

    public string CurrentFen = "2k4r/ppp2p2/3b1npp/2r1p3/2P3PN/P1QP1P2/1P1N2KP/n4R2 b - - 1 21";
    string prevFen = "";
    private const string StartingFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public Dictionary<char, PieceType> FenIds = new Dictionary<char, PieceType>
{
    { 'K', PieceType.King },
    { 'Q', PieceType.Queen },
    { 'B', PieceType.Bishop },
    { 'N', PieceType.Knight },
    { 'R', PieceType.Rook },
    { 'P', PieceType.Pawn }
};

    private Camera camera;
    public Vector2Int currentHover;

    void Awake()
    {
        StartCoroutine(GenerateAllTiles(TileSize, TileCountX, TileCountY));
        CurrentFen=StartingFen;
        SpawnAllpieces(CurrentFen);
    }

    void Update()
    {

        if (camera == null)
        { camera = Camera.main; return; }




        HighlightSquare();

    }

    private void HighlightSquare()
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit;
        hit = Physics2D.Raycast(ray.origin, ray.direction, 100, LayerMask.GetMask("Tile", "Hover"));

        if (hit && hit.collider != null)
        {
            Vector2Int hitTilePos = LoockupTileIndex(hit.transform.gameObject);

            //prevent highlighting if mouse is dragging a piece

            if (currentHover == -Vector2Int.one)
            {
                currentHover = hitTilePos;
                tiles[hitTilePos.x, hitTilePos.y].layer = LayerMask.NameToLayer("Hover");
                tiles[hitTilePos.x, hitTilePos.y].GetComponent<SpriteRenderer>().color = hoverColor;
            }

            if (currentHover != hitTilePos)
            {
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                tiles[currentHover.x, currentHover.y].GetComponent<SpriteRenderer>().color = (currentHover.x + currentHover.y) % 2 == 0 ? tileBlackColor : tileWhiteColor;
                currentHover = hitTilePos;
                tiles[hitTilePos.x, hitTilePos.y].layer = LayerMask.NameToLayer("Hover");
                tiles[hitTilePos.x, hitTilePos.y].GetComponent<SpriteRenderer>().color = hoverColor;
            }



            if (Input.GetMouseButtonDown(0))
            {
                currentlyDraggingPiece = chessPieces[hitTilePos.x, hitTilePos.y];
            }

            if (Input.GetMouseButton(0) && currentlyDraggingPiece != null)
            {
                MoveWithMouse(hitTilePos);
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (currentlyDraggingPiece != null)
                {
                    DragPiece(previousPiecePosition);
                }
            }


        }
        else
        {
            if (currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                tiles[currentHover.x, currentHover.y].GetComponent<SpriteRenderer>().color = (currentHover.x + currentHover.y) % 2 == 0 ? tileBlackColor : tileWhiteColor;
                currentHover = -Vector2Int.one;

            }
        }
    }

    private void MoveWithMouse(Vector2Int hitTilePos)
    {

        previousPiecePosition = new Vector2Int((int)currentlyDraggingPiece.transform.position.x, (int)currentlyDraggingPiece.transform.position.y);
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = camera.WorldToScreenPoint(currentlyDraggingPiece.transform.position).z;

        Vector3 worldPos = camera.ScreenToWorldPoint(mousePos);
        worldPos.z = currentlyDraggingPiece.transform.position.z;

        currentlyDraggingPiece.transform.position = worldPos;
    }

    private void DragPiece(Vector2Int previousPiecePos)
    {

        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit;
        hit = Physics2D.Raycast(ray.origin, ray.direction, 100, LayerMask.GetMask("Tile", "Hover"));
        Vector2Int hitTilePos = LoockupTileIndex(hit.transform.gameObject);
        currentlyDraggingPiece.transform.position = new Vector2(hitTilePos.x, hitTilePos.y);
        currentlyDraggingPiece.transform.parent = hit.transform;
        chessPieces[previousPiecePos.x, previousPiecePos.y] = null;
        chessPieces[hitTilePos.x, hitTilePos.y] = currentlyDraggingPiece;
        currentlyDraggingPiece = null;
    }


    private Vector2 GetCenterOfTile(Vector2Int tile)
    {
        return new Vector2(tile.x / 2, tile.y / 2);
    }

    //generate board
    private IEnumerator GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        tiles = new GameObject[tileCountX, tileCountY];

        for (int y = 0; y < tileCountX; y++)
            for (int x = 0; x < tileCountY; x++)
            {
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);
                yield return new WaitForSeconds(0.05f);
            }

        NameTiles();
    }

    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tile = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tile.transform.parent = transform;
        tile.AddComponent<SpriteRenderer>();
        bool isWhiteSquare = true;
        if ((x + y) % 2 == 0)
            isWhiteSquare = false;
        tile.GetComponent<SpriteRenderer>().sprite = tileSprite;
        tile.GetComponent<SpriteRenderer>().color = isWhiteSquare ? tileWhiteColor : tileBlackColor;

        tile.transform.localScale = new Vector3(tileSize, tileSize, tileSize);
        tile.transform.position = new Vector2(x * tileSize, y * tileSize);
        tile.AddComponent<BoxCollider2D>();
        tile.layer = LayerMask.NameToLayer("Tile");


        return tile;
    }

    //SpawnPieces

    private void SpawnAllpieces(string FenLayout)
    {
        string fenboard = FenLayout.Split(' ')[0];
        int file = 0, rank = 7;
        Team team;
        PieceType type;
        foreach (char symbol in fenboard)
        {
            if (symbol == '/')
            {
                file = 0;
                rank--;
            }
            else
            {
                if (char.IsDigit(symbol))
                    file += (int)char.GetNumericValue(symbol);

                else
                {
                    file++;
                    team = (char.IsUpper(symbol)) ? Team.White : Team.Black;
                    type = FenIds[char.ToUpper(symbol)];
                    SpawnSinglePiece(type, team, file, rank);
                }
            }

        }

    }

    private ChessPiece SpawnSinglePiece(PieceType type, Team team, int file, int rank)
    {
        ChessPiece cp;
        if (team == Team.Black)
            cp = Instantiate(blackPieces[(int)type]).GetComponent<ChessPiece>();
        else
            cp = Instantiate(whitePieces[(int)type]).GetComponent<ChessPiece>();

        cp.team = team;
        cp.type = type;
        cp.transform.position = new Vector2(file - 1, rank);
        cp.gameObject.AddComponent<BoxCollider2D>().isTrigger = true;
        chessPieces[file - 1, rank] = cp;
        return cp;
    }
    //Cell Rank ANd Files Numbering And Add Characters
    private void NameTiles(bool isBlackPlayer = false)
    {
        if (!isBlackPlayer)
        {
            // Add characters for ranks
            for (int i = 0; i < TileCountY; i++)
            {
                Color textColor = i % 2 == 0 ? Color.black : Color.white;
                CreateTextMeshPro(tiles[7, i].transform, (i + 1).ToString(),
                    new Vector3(7.45f, 0.375f + (TileSize * i), 0),
                    new Vector2(0.235f, 0.235f), textColor);
            }

            // Add characters for files
            int j = 0;
            for (char c = 'A'; c < 'I'; c++)
            {
                Color textColor = j % 2 == 0 ? Color.white : Color.black;
                CreateTextMeshPro(tiles[j, 0].transform, c.ToString(),
                    new Vector3(-0.3f + (TileSize * j), -0.375f, 0),
                    new Vector2(0.235f, 0.235f), textColor);
                j++;
            }
        }
        else
        {
            // TODO: Add characters for ranks and files for black player
        }
    }

    private GameObject CreateTextMeshPro(Transform parent, string text, Vector3 Position, Vector2 deltaSize, Color color, FontStyles style = FontStyles.Bold)
    {
        GameObject txt = new GameObject();
        txt.transform.parent = parent;
        txt.AddComponent<TextMeshPro>().text = text;
        txt.GetComponent<TextMeshPro>().fontSizeMin = 1;
        txt.GetComponent<TextMeshPro>().sortingOrder = 4;
        txt.GetComponent<TextMeshPro>().enableAutoSizing = true;
        txt.GetComponent<TextMeshPro>().fontStyle = style;
        txt.GetComponent<RectTransform>().position = Position;
        txt.GetComponent<RectTransform>().sizeDelta = deltaSize;
        txt.GetComponent<TextMeshPro>().color = color;
        return txt;

    }

    private Vector2Int LoockupTileIndex(GameObject HitTile)
    {
        for (int x = 0; x < TileCountX; x++)
            for (int y = 0; y < TileCountY; y++)
                if (tiles[x, y] == HitTile)
                    return new Vector2Int(x, y);

        return -Vector2Int.one;
    }


/*
    private void OnValidate()
    {
        if (Application.isPlaying && CurrentFen != prevFen)
        {
            for (int i = 0; i < chessPieces.GetLength(0); i++)
            {
                for (int j = 0; j < chessPieces.GetLength(1); j++)
                {
                    if (chessPieces[i, j] != null)
                    {
                        Destroy(chessPieces[i, j].gameObject);

                        chessPieces[i, j] = null;
                    }
                }
            }
            SpawnAllpieces(CurrentFen);
            prevFen = CurrentFen;

        }
    }
*/
}