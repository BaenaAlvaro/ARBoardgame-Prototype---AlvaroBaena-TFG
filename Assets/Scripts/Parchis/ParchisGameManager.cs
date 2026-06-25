using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Los dos estados posibles de cada jugador
public enum PlayerType { Human, Virtual }

// Las fases del turno
public enum TurnPhase { WaitingToRoll, WaitingToMove, GameOver }

public class ParchisGameManager : MonoBehaviour
{
    // ---- CONFIGURACIÓN EN EL INSPECTOR ----
    [Header("UI")]
    [SerializeField] private ParchisUI m_UI;

    [Header("Nombres de fichas en el prefab")]
    [SerializeField] private string m_HumanPiece1Name = "Ficha_H_01";
    [SerializeField] private string m_HumanPiece2Name = "Ficha_H_02";
    [SerializeField] private string m_VirtualPiece1Name = "Ficha_V_01";
    [SerializeField] private string m_VirtualPiece2Name = "Ficha_V_02";

    [Header("Virtual Player")]
    [SerializeField] private float m_VirtualPlayerDelay = 1.5f;

    // ---- ESTADO INTERNO ----
    private ParchisPiece[] m_HumanPieces;
    private ParchisPiece[] m_VirtualPieces;
    private ParchisBoard m_Board;

    private int m_CurrentPlayer = 0;
    private int m_DiceResult = 0;
    private TurnPhase m_CurrentPhase = TurnPhase.WaitingToRoll;
    private bool m_HasRolled = false;
    private bool m_GameReady = false;

    public UnityEvent<int> OnDiceRolled;
    public UnityEvent<string> OnMessageChanged;
    public UnityEvent<int> OnTurnChanged;

    // ---- LLAMADO POR ARBOARDSPAWNER CUANDO EL TABLERO APARECE ----
    public void OnBoardSpawned(GameObject board)
    {
        m_Board = board.GetComponentInChildren<ParchisBoard>();

        if (m_Board == null)
        {
            return;
        }

        Transform piece1H = board.transform.Find(m_HumanPiece1Name);
        Transform piece2H = board.transform.Find(m_HumanPiece2Name);
        Transform piece1V = board.transform.Find(m_VirtualPiece1Name);
        Transform piece2V = board.transform.Find(m_VirtualPiece2Name);

        if (piece1H == null || piece2H == null || piece1V == null || piece2V == null)
        {
            return;
        }

        m_HumanPieces = new ParchisPiece[]
        {
                piece1H.GetComponent<ParchisPiece>(),
                piece2H.GetComponent<ParchisPiece>()
        };

        m_VirtualPieces = new ParchisPiece[]
        {
                piece1V.GetComponent<ParchisPiece>(),
                piece2V.GetComponent<ParchisPiece>()
        };

        m_GameReady = true;
        StartTurn(0);
    }

    // ---- INICIO ----
    void Start()
    {
        OnMessageChanged?.Invoke("Apunta la cámara al tablero para empezar");
    }
    public void RollDice()
    {
        if (!m_GameReady) return;
        if (m_CurrentPlayer != 0 || m_HasRolled)
            return;

        m_DiceResult = Random.Range(1, 7);
        m_HasRolled = true;
        m_CurrentPhase = TurnPhase.WaitingToMove;

        OnDiceRolled?.Invoke(m_DiceResult);

        CheckDiceResult();
    }

    private void CheckDiceResult()
    {
        bool canDoSomething = false;

        if (m_DiceResult == 5)
        {
            foreach (var piece in m_HumanPieces)
            {
                if (piece.IsAtHome())
                {
                    canDoSomething = true;
                    break;
                }
            }
            foreach (var piece in m_HumanPieces)
            {
                if (!piece.IsAtHome() && !piece.HasFinished())
                {
                    canDoSomething = true;
                    break;
                }
            }
        }
        else
        {
            foreach (var piece in m_HumanPieces)
            {
                if (!piece.IsAtHome() && !piece.HasFinished())
                {
                    canDoSomething = true;
                    break;
                }
            }
        }
        if (canDoSomething)
        {
            if (m_DiceResult == 5)
                OnMessageChanged?.Invoke("Sacaste 5. Puedes sacar ficha de casa o mover. Toca la ficha que quieras mover.");
            else
                OnMessageChanged?.Invoke($"Sacaste {m_DiceResult}. Toca la ficha que quieres mover.");
        }
        else
        {
            OnMessageChanged?.Invoke($"Sacaste {m_DiceResult}. Sin movimientos posibles. Pasando turno...");
            StartCoroutine(AutoPassTurn(1f));
        }
    }

    public void OnPieceSelected(ParchisPiece piece)
    {
        if (m_CurrentPlayer != 0 || !m_HasRolled)
            return;

        if (!IsHumanPiece(piece))
            return;

        if (piece.IsAtHome() && m_DiceResult == 5)
        {
            piece.ExitHome();
            OnMessageChanged?.Invoke("Ficha sacada de casa. Pulsa Pasar Turno.");
            m_CurrentPhase = TurnPhase.WaitingToRoll;
            return;
        }

        if (!piece.IsAtHome() && !piece.HasFinished())
        {
            int targetSquare = m_Board.GetTargetSquare(piece.CurrentSquare, m_DiceResult, 0);

            ParchisPiece capturedPiece = m_Board.GetPieceAtSquare(targetSquare, GetVirtualPieces());
            if (capturedPiece != null && !m_Board.IsSafeSquare(targetSquare))
            {
                capturedPiece.SendHome();
                OnMessageChanged?.Invoke($"Captura! La ficha enemiga vuelve a casa. Pulsa Pasar Turno.");
            }
            else
            {
                OnMessageChanged?.Invoke($"Ficha movida {m_DiceResult} casillas. Pulsa Pasar Turno.");
            }

            piece.MoveTo(targetSquare);
            m_CurrentPhase = TurnPhase.WaitingToRoll;
            CheckVictory();
        }
    }
    public void PassTurn()
    {
        if (!m_GameReady) return;

        if (m_CurrentPlayer != 0 || m_CurrentPhase != TurnPhase.WaitingToRoll || !m_HasRolled)
            return;

        StartTurn(1);
    }
    private void StartTurn(int player)
    {
        m_CurrentPlayer = player;
        m_HasRolled = false;
        m_CurrentPhase = TurnPhase.WaitingToRoll;
        OnTurnChanged?.Invoke(player);

        if (player == 0)
        {
            OnMessageChanged?.Invoke("Tu turno. Pulsa Tirar Dado.");
        }
        else
        {
            OnMessageChanged?.Invoke("Turno del rival. Espera...");
            StartCoroutine(VirtualPlayerTurn());
        }
    }

    private IEnumerator VirtualPlayerTurn()
    {
        yield return new WaitForSeconds(m_VirtualPlayerDelay);

        int dice = Random.Range(1, 7);
        OnDiceRolled?.Invoke(dice);
        OnMessageChanged?.Invoke($"El rival sacó {dice}.");

        yield return new WaitForSeconds(m_VirtualPlayerDelay);

        bool moved = false;

        int piecesAtHome = 0;
        int piecesOnBoard = 0;

        foreach (var piece in m_VirtualPieces)
        {
            if (piece.IsAtHome()) piecesAtHome++;
            else if (!piece.HasFinished()) piecesOnBoard++;
        }

        if (piecesOnBoard > 0)
        {
            foreach (var piece in m_VirtualPieces)
            {
                if (!piece.IsAtHome() && !piece.HasFinished())
                {
                    int targetSquare = m_Board.GetTargetSquare(
                        piece.CurrentSquare, dice, 1);

                    ParchisPiece capturedPiece = m_Board.GetPieceAtSquare(
                        targetSquare, GetHumanPieces());

                    if (capturedPiece != null && !m_Board.IsSafeSquare(targetSquare))
                    {
                        capturedPiece.SendHome();
                        OnMessageChanged?.Invoke(
                            "El rival capturó una de tus fichas!");
                    }
                    else
                    {
                        OnMessageChanged?.Invoke(
                            $"El rival movió {dice} casillas.");
                    }

                    piece.MoveTo(targetSquare);
                    moved = true;
                    CheckVictory();
                    break;
                }
            }
        }

        if (!moved && dice == 5 && piecesAtHome > 0)
        {
            foreach (var piece in m_VirtualPieces)
            {
                if (piece.IsAtHome())
                {
                    piece.ExitHome();
                    OnMessageChanged?.Invoke("El rival sacó ficha de casa.");
                    moved = true;
                    break;
                }
            }
        }

        if (!moved)
        {
            if (piecesAtHome == m_VirtualPieces.Length)
                OnMessageChanged?.Invoke(
                    $"El rival sacó {dice}. Necesita un 5 para salir. Tu turno.");
            else
                OnMessageChanged?.Invoke(
                    $"El rival sacó {dice} pero no puede mover. Tu turno.");
        }

        yield return new WaitForSeconds(m_VirtualPlayerDelay);
        StartTurn(0);

    }

    // ---- HELPERS ----
    private bool IsHumanPiece(ParchisPiece piece)
    {
        foreach (var p in m_HumanPieces)
            if (p == piece) return true;
        return false;
    }

    private ParchisPiece[] GetHumanPieces() => m_HumanPieces;
    private ParchisPiece[] GetVirtualPieces() => m_VirtualPieces;

    private IEnumerator AutoPassTurn(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartTurn(1);
    }

    // ---- COMPROBAR VICTORIA ----
    private void CheckVictory()
    {
        bool humanWins = true;
        foreach (var piece in m_HumanPieces)
        {
            if (!piece.HasFinished())
            {
                humanWins = false;
                break;
            }
        }

        bool virtualWins = true;
        foreach (var piece in m_VirtualPieces)
        {
            if (!piece.HasFinished())
            {
                virtualWins = false;
                break;
            }
        }

        if (humanWins)
        {
            m_CurrentPhase = TurnPhase.GameOver;
            OnMessageChanged?.Invoke("¡HAS GANADO! Enhorabuena!");
            OnTurnChanged?.Invoke(-1); // -1 desactiva los botone
        }
        else if (virtualWins)
        {
            m_CurrentPhase = TurnPhase.GameOver;
            OnMessageChanged?.Invoke("El rival ha ganado. ¡Suerte la próxima!");
            OnTurnChanged?.Invoke(-1);
        }
    }
}
