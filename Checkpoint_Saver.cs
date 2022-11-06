using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using KinematicCharacterController.Examples;

[System.Serializable]
public class Checkpoint_Saver : MonoBehaviour
{
    public GameSaver.chapter Checkpoint;
    public Transform PlayerSpawnPosition;
    public bool KeepRotation;
    GameManager gm;
    GameSaver _GS = new GameSaver();
    public string whatSceneToLoad;


    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        if(GameObject.Find("__app") != null)
            gm = GameObject.Find("__app").GetComponentInChildren<GameManager>();
        else
            Debug.LogWarning("The __app was not found. load the preload to load it into the scene");
    }

    /// <summary>Saves the current save data, and the chapter of this checkpoint</summary>
    public void SaveCheckpoint()
    {
        if(gm != null)
        {
            gm.saveData.currentChapter = Checkpoint;
            gm.saveData.currentScene = SceneManager.GetActiveScene().buildIndex;
            Debug.Log("saving" + gm.saveData.currentChapter);
            _GS.SaveGame(gm.saveData);
        }
        else
            Debug.LogWarning("The GameManager was not found. load the preload to get a GameManager into the scene");
    }   


    /// <summary>Teleports the player to this checkpoint</summary>
    public void Spawn()
    {
        if(gm != null)
        {
            
            gm.characterController.Motor.SetPositionAndRotation(PlayerSpawnPosition.transform.position, PlayerSpawnPosition.transform.rotation);
            if(!KeepRotation)
                gm.characterController.Motor.SetPositionAndRotation(PlayerSpawnPosition.transform.position, Quaternion.identity);
            // Debug.Log("Player: " + gm._player.transform.position + " - Spawn: " + PlayerSpawnPosition.transform.position);
        }
        else
            Debug.LogWarning("The GameManager was not found. load the preload to get a GameManager into the scene");
    }

    public void LoadNextScene(string SceneToLoad)
    {
        try
        {
            int sceneInt = int.Parse(SceneToLoad);
            SceneManager.LoadScene(sceneInt);
            // Debug.Log("try");
        }
        catch
        {
            Debug.Log("catch: " + SceneToLoad);
            SceneManager.LoadScene(SceneToLoad);
            // Debug.Log("catch");
        }
        // Debug.Log("end");
    }

    private void OnTriggerEnter(Collider other)
    {
        if(whatSceneToLoad != "")
        {
            if (other.tag == "Player")
            {

                SaveCheckpoint();
                LoadNextScene(whatSceneToLoad);

            }
        }
    }
}