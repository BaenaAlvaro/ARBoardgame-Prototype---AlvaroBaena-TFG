using UnityEngine;
using UnityEditor;

public class BoardSetupHelper : EditorWindow
{
    private GameObject m_BoardObject;

    [MenuItem("Parchis/Setup Board Squares")]
    public static void ShowWindow()
    {
        GetWindow<BoardSetupHelper>("Setup Tablero");
    }

    void OnGUI()
    {
        GUILayout.Label("Generador de Casillas", EditorStyles.boldLabel);

        m_BoardObject = (GameObject)EditorGUILayout.ObjectField(
            "Tablero", m_BoardObject, typeof(GameObject), true);

        if (m_BoardObject == null)
        {
            EditorGUILayout.HelpBox(
                "Arrastra tu tablero aquí", MessageType.Info);
            return;
        }

        if (GUILayout.Button("Crear Casillas Principales"))
            CreateMainSquares();

        if (GUILayout.Button("Crear Pasillo Final Humano"))
            CreateFinalPath("PasilloFinal_Humano", "PF_H_", 6);

        if (GUILayout.Button("Crear Pasillo Final Virtual"))
            CreateFinalPath("PasilloFinal_Virtual", "PF_V_", 6);

        if (GUILayout.Button("Crear Metas y Casas"))
            CreateGoalsAndHomes();
    }

    private void CreateMainSquares()
    {
        Transform container = GetOrCreateChild(m_BoardObject.transform, "Casillas");

        for (int i = 0; i < 68; i++)
        {
            string name = $"Casilla_{i:D2}";
            if (container.Find(name) != null) continue;

            GameObject square = new GameObject(name);
            square.transform.SetParent(container);
            square.transform.localPosition = Vector3.zero;
        }
    }

    private void CreateFinalPath(string containerName, string prefix, int count)
    {
        Transform container = GetOrCreateChild(m_BoardObject.transform, containerName);

        for (int i = 0; i < count; i++)
        {
            string name = $"{prefix}{i:D2}";
            if (container.Find(name) != null) continue;

            GameObject square = new GameObject(name);
            square.transform.SetParent(container);
            square.transform.localPosition = Vector3.zero;
        }
    }

    private void CreateGoalsAndHomes()
    {
        // Metas
        CreateSingleIfNotExists(m_BoardObject.transform, "Meta_Humano");
        CreateSingleIfNotExists(m_BoardObject.transform, "Meta_Virtual");

        // Posiciones iniciales de las fichas
        Transform homesHuman = GetOrCreateChild(
            m_BoardObject.transform, "Casas_Humano");
        Transform homesVirtual = GetOrCreateChild(
            m_BoardObject.transform, "Casas_Virtual");

        for (int i = 0; i < 2; i++)
        {
            CreateSingleIfNotExists(homesHuman, $"Casa_H_{i}");
            CreateSingleIfNotExists(homesVirtual, $"Casa_V_{i}");
        }
    }

    private Transform GetOrCreateChild(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null) return existing;

        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;
        return go.transform;
    }

    private void CreateSingleIfNotExists(Transform parent, string name)
    {
        if (parent.Find(name) != null) return;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;
    }
}
