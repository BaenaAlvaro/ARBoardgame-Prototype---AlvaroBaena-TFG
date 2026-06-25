using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ParchisUI : MonoBehaviour
{
    // ---- CONFIGURACIÓN EN EL INSPECTOR ----
    [Header("Botones")]
    [SerializeField] private Button m_RollDiceButton;
    [SerializeField] private Button m_PassTurnButton;

    [Header("Textos")]
    [SerializeField] private TMP_Text m_DiceResultText;
    [SerializeField] private TMP_Text m_MessageText;
    [SerializeField] private TMP_Text m_TurnText;

    [Header("Indicadores visuales")]
    [SerializeField] private GameObject m_HumanTurnIndicator;
    [SerializeField] private GameObject m_VirtualTurnIndicator;

    [Header("Dado visual")]
    [SerializeField] private Sprite[] m_DiceFaces;
    [SerializeField] private Image m_DiceImage;

    private ParchisGameManager m_GameManager;

    void Start()
    {
        m_GameManager = FindFirstObjectByType<ParchisGameManager>();

        if (m_GameManager == null)
        {
            return;
        }

        m_RollDiceButton.onClick.AddListener(m_GameManager.RollDice);
        m_PassTurnButton.onClick.AddListener(m_GameManager.PassTurn);

        m_GameManager.OnDiceRolled.AddListener(UpdateDiceDisplay);
        m_GameManager.OnMessageChanged.AddListener(UpdateMessage);
        m_GameManager.OnTurnChanged.AddListener(UpdateTurnDisplay);

        SetButtonsForRollPhase();
    }

    void OnDestroy()
    {
        if (m_GameManager != null)
        {
            m_GameManager.OnDiceRolled.RemoveListener(UpdateDiceDisplay);
            m_GameManager.OnMessageChanged.RemoveListener(UpdateMessage);
            m_GameManager.OnTurnChanged.RemoveListener(UpdateTurnDisplay);
        }
    }

    private void UpdateDiceDisplay(int diceValue)
    {

        if (m_DiceResultText != null)
        { 
          m_DiceResultText.text = diceValue.ToString(); 
        }
            

        // Si tenemos sprites del dado, mostramos la cara correcta
        if (m_DiceImage != null && m_DiceFaces != null && m_DiceFaces.Length == 6)
            m_DiceImage.sprite = m_DiceFaces[diceValue - 1];

        SetButtonsForMovePhase();
    }

    private void UpdateMessage(string message)
    {
        if (m_MessageText != null)
            m_MessageText.text = message;
    }

    private void UpdateTurnDisplay(int playerIndex)
    {
        if (playerIndex == -1)
        {
            SetButtonsForVirtualTurn();
            if (m_TurnText != null)
                m_TurnText.text = "Partida terminada";
            if (m_HumanTurnIndicator != null)
                m_HumanTurnIndicator.SetActive(false);
            if (m_VirtualTurnIndicator != null)
                m_VirtualTurnIndicator.SetActive(false);
            return;
        }

        bool isHumanTurn = playerIndex == 0;
        if (m_HumanTurnIndicator != null)
            m_HumanTurnIndicator.SetActive(isHumanTurn);
        if (m_VirtualTurnIndicator != null)
            m_VirtualTurnIndicator.SetActive(!isHumanTurn);
        if (m_TurnText != null)
            m_TurnText.text = isHumanTurn ? "Tu turno" : "Turno del rival";
        if (isHumanTurn)
            SetButtonsForRollPhase();
        else
            SetButtonsForVirtualTurn();
    }

    private void SetButtonsForRollPhase()
    {
        if (m_RollDiceButton != null)
            m_RollDiceButton.interactable = true;

        if (m_PassTurnButton != null)
            m_PassTurnButton.interactable = false;
    }

    private void SetButtonsForMovePhase()
    {
        if (m_RollDiceButton != null)
            m_RollDiceButton.interactable = false;

        if (m_PassTurnButton != null)
            m_PassTurnButton.interactable = true;
    }

    private void SetButtonsForVirtualTurn()
    {
        if (m_RollDiceButton != null)
            m_RollDiceButton.interactable = false;

        if (m_PassTurnButton != null)
            m_PassTurnButton.interactable = false;
    }
}
