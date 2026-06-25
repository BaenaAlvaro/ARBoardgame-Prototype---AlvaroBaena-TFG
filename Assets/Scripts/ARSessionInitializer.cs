using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Android;

public class ARSessionInitializer : MonoBehaviour
{
    [SerializeField] private ARSession m_ARSession;
    [SerializeField] private ARCameraManager m_CameraManager;

    IEnumerator Start()
    {
        // Primero pedimos permiso de cámara
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
            // Esperamos 2 segundos a que el usuario responda el popup
            yield return new WaitForSeconds(2f);
        }

        // Desactivamos y reactivamos el AR Session para forzar reinicio
        m_ARSession.gameObject.SetActive(false);
        yield return new WaitForEndOfFrame();
        m_ARSession.gameObject.SetActive(true);

        // Esperamos a que ARCore esté listo
        yield return new WaitUntil(() =>
            ARSession.state == ARSessionState.SessionTracking ||
            ARSession.state == ARSessionState.SessionInitializing);

        Debug.Log($"AR Session iniciado. Estado: {ARSession.state}");
    }
}
