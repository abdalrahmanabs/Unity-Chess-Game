
using UnityEngine;
using System;
using System.Collections.Generic;

public enum PieceType
{
   Pawn, Rook, Knight, Bishop, King, Queen
}

public enum Team
{

    White, Black
}
public class ChessPiece : MonoBehaviour
{
    //columnt=file
    //row=rank

    public string FENId { get; }
   



    public PieceType type;
    public Team team;
    public int value;
    public string currentFile = "";
    public int currentRank = 0;

    private Vector3 desiredPos;
    private Vector3 desiredScale;

}
