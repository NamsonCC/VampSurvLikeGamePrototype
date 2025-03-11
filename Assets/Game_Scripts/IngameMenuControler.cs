using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Scenes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IngameMenuControler : MonoBehaviour
{
    [SerializeField] private GameObject GameOverMenu;
    [SerializeField] private GameObject FillableImage;



    public static IngameMenuControler Instance;
    private Unity.Entities.Hash128 subSceneHash;

    public bool NewWorldCreated;

    public List<EntitySceneReference> LevelScenes = new();
    private void Awake()
    {
        NewWorldCreated = true;
        Instance = this;
    }
    private void Start()
    {
        GameOverMenu.SetActive(false);
    }
    public void EnableGameOverMenu()
    {
        GameOverMenu.SetActive(true);
    }
    public void QuitFromGame()
    {
        Application.Quit();
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
    }
    public void RestartGame()
    {
        StartCoroutine(SafeRestartGame());
    }

    private IEnumerator SafeRestartGame()
    {

        NewWorldCreated = false;
          SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        Unity.Entities.Hash128 hashPath = SceneReferanceController.Instance.SceneData.sceneGUID;
        // Unity.Entities.Hash128 path = SceneSystem.GetSceneGUID(SystemState state, SceneReferanceController.Instance.subScenejsonPath);

        SceneSystem.UnloadScene(World.DefaultGameObjectInjectionWorld.Unmanaged, SceneSystem.GetSceneEntity(World.DefaultGameObjectInjectionWorld.Unmanaged, hashPath),
            SceneSystem.UnloadParameters.DestroyMetaEntities);

        World.DisposeAllWorlds();

        Debug.LogError("SceneManager.LoadScene;");
        DefaultWorldInitialization.Initialize("Default World", false);

        if (SceneReferanceController.Instance.loadFromScript)
        {
            /*   Unity.Entities.Hash128 hashPath = SceneReferanceController.Instance.SceneData.sceneGUID;
               // Unity.Entities.Hash128 path = SceneSystem.GetSceneGUID(SystemState state, SceneReferanceController.Instance.subScenejsonPath);

               SceneSystem.UnloadScene(World.DefaultGameObjectInjectionWorld.Unmanaged, SceneSystem.GetSceneEntity(World.DefaultGameObjectInjectionWorld.Unmanaged, hashPath),
                   SceneSystem.UnloadParameters.DestroyMetaEntities);*/
            yield return null;
            World.DisposeAllWorlds();

            // Yeni bir World oluþtur ve sistemleri baþlat
            DefaultWorldInitialization.Initialize("Default World", false);
            Debug.Log("SceneManager.LoadScene2; " + hashPath);

            Entity sceneEntityToLoad = SceneSystem.LoadSceneAsync(World.DefaultGameObjectInjectionWorld.Unmanaged, hashPath);
            Debug.Log("SceneManager.LoadScene3; " + hashPath);
        }
        else
        {
            var subscene = SubScene.Instantiate(SceneReferanceController.Instance.ScenePrefab);
        }

        NewWorldCreated = true;  


    }

}


