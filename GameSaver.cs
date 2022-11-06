using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameSaver
{
    public enum chapter
    {
        CarCrash,
        Bridge_left,
        Bridge_right,
        Cave_left,
        Cave_right,
        AnimalTerretory_left,
        AnimalTerretory_right,
        Sawmill_left,
        Sawmill_right,
        teleport1,
        teleport2
    };

    [System.Serializable] // temporary serializable
    public struct gameSave
    {
        public chapter currentChapter;
        public bool hasBearSpray;
        public bool hasKey;
        public int currentScene;

    };

    /// <summary>Save all data, Currect chapter, has BearSpray</summary>
    public void SaveGame(gameSave gameData)
    {
        saveChapter(gameData.currentChapter, gameData.currentScene);
        saveBearSpray(gameData.hasBearSpray);
    }

    /// <summary>reset the save back to the start of the game</summary>
    public void deleteSave()    
    {
        saveChapter(chapter.CarCrash, 2);
        saveBearSpray(false);
    }

    /// <summary>Load save data</summary>
    public gameSave loadSave()
    {
        gameSave loaded;
        loaded.currentChapter = loadChapter();
        loaded.hasBearSpray = loadBearSpray();
        loaded.hasKey = false;
        loaded.currentScene = 0;
        return loaded;
    }

    #region Save properties

    /// <summary>save chapter the player is currently in</summary>
    /// <param name="currentChapter">the chapter the player is in</param>
    public void saveChapter(chapter currentChapter, int scene)
    {
        PlayerPrefs.SetString("chapter", currentChapter.ToString());
        PlayerPrefs.SetInt("Scene", scene);
    }

    /// <summary>Saves if the player has bearspray or not</summary>
    /// <param name="hasSpray">0 - false, 1 - true</param>
    public void saveBearSpray(bool hasSpray)
    {
        int saving = 0;
        if(hasSpray)
            saving = 1;
         PlayerPrefs.SetInt("bearSpray", saving);
    }

    #endregion
    #region Load properties

    /// <summary>returns the loaded chapter</summary>
    public chapter loadChapter()
    {
        chapter loaded = chapter.CarCrash;
        if(PlayerPrefs.GetString("chapter") != "")
            loaded = (chapter) Enum.Parse(typeof(chapter), PlayerPrefs.GetString("chapter"));
        return loaded; 
    }

    /// <summary>gets if the player has bearspray</summary>
    public bool loadBearSpray()
    {
        if(PlayerPrefs.GetInt("bearSpray") == 0)
            return false;
        return true;
    }
    #endregion
}
