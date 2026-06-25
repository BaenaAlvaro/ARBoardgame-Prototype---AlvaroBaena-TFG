using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class BoardSpawnner : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private ARTrackedImageManager m_TrackedImageManager;
    [SerializeField] private GameObject m_BoardPrefab;
    [SerializeField] private string m_TargetImageName = "T_Parchis";

    [Header("References")]
    [SerializeField] private Transform m_GameAnchor;

    private GameObject m_SpawnedBoard;
    private bool m_BoardSpawned = false;

    void OnEnable()
    {
        m_TrackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
    }

    void OnDisable()
    {
        m_TrackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
    }

    private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            HandleTrackedImage(trackedImage);
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            HandleTrackedImage(trackedImage);
        }

        foreach (var trackedImage in eventArgs.removed)
        {
            if (trackedImage.Value.referenceImage.name == m_TargetImageName)
            {
                HandleImageLost();
            }
        }
    }

    private void HandleTrackedImage(ARTrackedImage trackedImage)
    {
        if (trackedImage.referenceImage.name != m_TargetImageName)
            return;

        if (trackedImage.trackingState == TrackingState.None)
            return;

        if (m_BoardSpawned)
        {
            UpdateBoardPosition(trackedImage);
            return; // cosa que evita llamar a SpawnBoard una segunda vez
        }
        SpawnBoard(trackedImage);
    }

    private void SpawnBoard(ARTrackedImage trackedImage)
    {
        if (m_BoardSpawned)
        {
            return;
        }

        if (m_BoardPrefab == null)
        {
            return;
        }

        m_GameAnchor.position = trackedImage.transform.position;
        m_GameAnchor.rotation = trackedImage.transform.rotation;

        m_SpawnedBoard = Instantiate(m_BoardPrefab, m_GameAnchor);
        m_SpawnedBoard.transform.localPosition = Vector3.zero;
        m_SpawnedBoard.transform.localRotation = Quaternion.identity;

        m_BoardSpawned = true;

        ParchisGameManager gameManager = FindFirstObjectByType<ParchisGameManager>();
        if (gameManager != null)
            gameManager.OnBoardSpawned(m_SpawnedBoard);
    }

    private void UpdateBoardPosition(ARTrackedImage trackedImage)
    {
        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            m_GameAnchor.position = trackedImage.transform.position;
            m_GameAnchor.rotation = trackedImage.transform.rotation;
        }
    }

    private void HandleImageLost()
    {
    }

    public void ResetBoard()
    {
        if (m_SpawnedBoard != null)
        {
            Destroy(m_SpawnedBoard);
            m_SpawnedBoard = null;
        }
        m_BoardSpawned = false;
    }
}