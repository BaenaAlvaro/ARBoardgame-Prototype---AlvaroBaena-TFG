using UnityEngine;

public class ParchisBoard : MonoBehaviour
{

    [Header("Casillas del tablero")]
    [SerializeField] private Transform[] m_Squares;

    [Header("Pasillos finales")]
    [SerializeField] private Transform[] m_HumanFinalPath;
    [SerializeField] private Transform[] m_VirtualFinalPath;

    [Header("Metas")]
    [SerializeField] private Transform m_HumanGoal;
    [SerializeField] private Transform m_VirtualGoal;

    private readonly int[] m_SafeSquares = { 0, 5, 8, 13, 17, 22, 26, 31, 34, 39, 43, 48, 52, 57, 60, 65 };

    // ---- OBTENER POSICIÓN FÍSICA DE UNA CASILLA ----
    public Transform GetSquarePosition(int squareIndex)
    {
        // Meta
        if (squareIndex == 99)
        {
            return m_HumanGoal;
        }

        // Pasillo final humano
        if (squareIndex >= 100 && squareIndex <= 106)
        {
            int index = squareIndex - 100;
            if (index < m_HumanFinalPath.Length)
                return m_HumanFinalPath[index];
            else
            {
                return m_HumanGoal;
            }
        }

        // Pasillo final virtual
        if (squareIndex >= 110 && squareIndex <= 116)
        {
            int index = squareIndex - 110;

            if (index < m_VirtualFinalPath.Length)
                return m_VirtualFinalPath[index];
            else
            {
                return m_VirtualGoal;
            }
        }

        // Circuito normal
        if (squareIndex >= 0 && squareIndex < m_Squares.Length)
            return m_Squares[squareIndex];

        return null;

    }

    // ---- CALCULAR CASILLA DESTINO ----
    public int GetTargetSquare(int currentSquare, int diceValue, int playerIndex)
    {
        if (playerIndex == 0)
        {

            if (currentSquare >= 100 && currentSquare < 107)
            {
                int finalTarget = currentSquare + diceValue;
                if (finalTarget >= 107)
                    return 99;
                return finalTarget;
            }

            // Detecta entrada al pasillo
            if (currentSquare <= 66 && currentSquare + diceValue > 67)
            {
                // Cuántos pasos entró dentro del pasillo
                int stepsIntoFinal = (currentSquare + diceValue) - 68;
                return 100 + stepsIntoFinal;
            }

            int target = currentSquare + diceValue;
            if (target > 67)
                target = target - 68;
            return target;
        }
        else
        {
            if (currentSquare >= 110 && currentSquare < 117)
            {
                int finalTarget = currentSquare + diceValue;
                if (finalTarget >= 117)
                    return 99; 
                return finalTarget;
            }

            if (currentSquare >= 55 && currentSquare <= 67)
            {
                int stepsToWrap = 68 - currentSquare;
                int stepsFromZeroToFinal = 51;
                int totalStepsToFinal = stepsToWrap + stepsFromZeroToFinal;

                if (diceValue > totalStepsToFinal)
                {
                    int stepsIntoFinal = diceValue - totalStepsToFinal;
                    return 110 + stepsIntoFinal;
                }
                if (diceValue == totalStepsToFinal)
                    return 110;

                int target = currentSquare + diceValue;
                if (target > 67)
                    target = target - 68;
                return target;
            }

            if (currentSquare >= 0 && currentSquare <= 50)
            {
                if (currentSquare + diceValue > 50)
                {
                    int stepsIntoFinal = (currentSquare + diceValue) - 51;
                    return 110 + stepsIntoFinal;
                }
            }

            int target2 = currentSquare + diceValue;
            if (target2 > 67)
                target2 = target2 - 68;
            return target2;
        }
    }

    public ParchisPiece GetPieceAtSquare(int squareIndex, ParchisPiece[] enemyPieces)
    {
        foreach (var piece in enemyPieces)
        {
            if (!piece.IsAtHome() && !piece.HasFinished())
            {
                if (piece.CurrentSquare == squareIndex)
                    return piece;
            }
        }
        return null;
    }

    public bool IsSafeSquare(int squareIndex)
    {
        foreach (int safeSquare in m_SafeSquares)
            if (squareIndex == safeSquare)
                return true;
        return false;
    }

    void OnDrawGizmos()
    {
        if (m_Squares != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var square in m_Squares)
                if (square != null)
                    Gizmos.DrawSphere(square.position, 0.02f);
        }

        if (m_HumanFinalPath != null)
        {
            Gizmos.color = Color.green;
            foreach (var square in m_HumanFinalPath)
                if (square != null)
                    Gizmos.DrawSphere(square.position, 0.02f);
        }

        if (m_VirtualFinalPath != null)
        {
            Gizmos.color = Color.red;
            foreach (var square in m_VirtualFinalPath)
                if (square != null)
                    Gizmos.DrawSphere(square.position, 0.02f);
        }

        if (m_HumanGoal != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(m_HumanGoal.position, 0.04f);
        }
    }
}