using UnityEngine;
using System.Collections;

public class ParchisPiece : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int m_StartSquare = 0;
    [SerializeField] private float m_MoveSpeed = 0.3f;

    [Header("Estado inicial")]
    [SerializeField] private Transform m_HomePosition;

    [Header("Ficha física")]
    [SerializeField] private bool m_IsPhysicalPiece = false;

    private int m_CurrentSquare = -1;

    private bool m_IsAtHome = true;

    private bool m_HasFinished = false;

    private ParchisGameManager m_GameManager;

    // ---- PROPIEDADES ----
    public int CurrentSquare => m_CurrentSquare;

    void Start()
    {
        m_GameManager = FindFirstObjectByType<ParchisGameManager>();
        SendHome();
        if (m_IsPhysicalPiece)
        {
            SetVisibility(false);
        }
    }

    void OnMouseDown()
    {
        m_GameManager?.OnPieceSelected(this);
    }

    public void ExitHome()
    {
        m_IsAtHome = false;
        m_CurrentSquare = m_StartSquare;

        Transform targetPosition = FindFirstObjectByType<ParchisBoard>()
            .GetSquarePosition(m_CurrentSquare);

        StartCoroutine(MoveToPosition(targetPosition.position));
    }

    public void MoveTo(int targetSquare)
    {
        m_CurrentSquare = targetSquare;

        if (targetSquare == 99)
        {
            m_HasFinished = true;
            Transform goalPos = FindFirstObjectByType<ParchisBoard>().GetSquarePosition(99);
            if (goalPos != null)
                StartCoroutine(MoveToPosition(goalPos.position));
            return;
        }


        Transform targetPosition = FindFirstObjectByType<ParchisBoard>()
            .GetSquarePosition(targetSquare);

        if (targetPosition != null)
        {
            StartCoroutine(MoveToPosition(targetPosition.position));
        }
        else
        {
        }
    }

    public void SendHome()
    {
        m_IsAtHome = true;
        m_CurrentSquare = -1;
        m_HasFinished = false;

        if (m_HomePosition != null)
            transform.position = m_HomePosition.position;
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < m_MoveSpeed)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / m_MoveSpeed;

            // Lerp mueve suavemente entre dos puntos
            transform.position = Vector3.Lerp(startPosition, targetPosition, progress);

            yield return null;
        }
        transform.position = targetPosition;
    }

    public bool IsAtHome() => m_IsAtHome;
    public bool HasFinished() => m_HasFinished;

    private void SetVisibility(bool visible)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            if (renderer.gameObject.name == "Shadow")
                continue;

            renderer.enabled = visible;
        }
    }
}
