﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour
{
    public GameObject settingsMenu;

    void Update()
    {
        if ( Input.GetKeyDown( KeyCode.O ) )
        {
            settingsMenu.gameObject.SetActive( true );
        }
    }

    public void ReturnToGame()
    {
        settingsMenu.gameObject.SetActive( false );
    }
}
