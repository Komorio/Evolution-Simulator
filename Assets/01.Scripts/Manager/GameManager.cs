using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : DontDestroySingletonObject<GameManager> {
    private void Awake() {
        Debug.Log("Started");
    }
}
