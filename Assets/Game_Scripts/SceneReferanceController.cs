
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.IO;
using Unity.Entities;
using Unity.Scenes;
using System.Collections;

[System.Serializable]
public class SceneData
{
    public Unity.Entities.Hash128 sceneGUID;
    public string sceneName;
}

public class SceneReferanceController : MonoBehaviour
{
    public bool loadFromScript;
    public GameObject SceneGamobject;
    public GameObject ScenePrefab;
    public static SceneReferanceController Instance;

#if UNITY_EDITOR
    public SceneAsset scene;
#endif
    public string subScenejsonPath;
    public SceneData SceneData;

    private void Start()
    {
        Instance = this;
        subScenejsonPath = Path.Combine(Application.persistentDataPath, "sceneData.json");

#if UNITY_EDITOR
        if (scene != null)
        {
            string scenePath = AssetDatabase.GetAssetPath(scene);
            Unity.Entities.Hash128 sceneGUID = AssetDatabase.GUIDFromAssetPath(scenePath);
            string sceneName = Path.GetFileNameWithoutExtension(scenePath);

            SceneData data = new SceneData { sceneGUID = sceneGUID, sceneName = sceneName };
            string json = JsonUtility.ToJson(data, true);

            File.WriteAllText(subScenejsonPath, json);
        }
#endif
        SceneLoader();
    }
    public void SceneLoader()
    {
        string jsonPath;


        jsonPath = Path.Combine(Application.persistentDataPath, "sceneData.json");

        if (File.Exists(jsonPath))
        {
            string json = File.ReadAllText(jsonPath);
            SceneData = JsonUtility.FromJson<SceneData>(json);
            subScenejsonPath = jsonPath;
            if (!string.IsNullOrEmpty(SceneData.sceneName))
            {
                Debug.Log("Scene name: " + SceneData.sceneName + " SceneData.sceneGUID; " + SceneData.sceneGUID);
            }
            else
            {
                Debug.LogError("Scene name is missing in JSON!");
            }
            StartCoroutine(WaitToWorldCreation());
        }
        else
        {
            Debug.LogError("Scene data JSON file not found!");
        }
        Debug.Log("data.hashPath: " + SceneData.sceneGUID);


    }

    private IEnumerator WaitToWorldCreation()
    {
        while (IngameMenuControler.Instance.NewWorldCreated == false)
        {
            yield return null;
        }
        if (loadFromScript == true)
        {
            SceneSystem.LoadSceneAsync(World.DefaultGameObjectInjectionWorld.Unmanaged, SceneData.sceneGUID);
        }

    }




}
