using Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IManager
{
    void RegisterManager(GameManager mgr);
    void ShutdownManager(GameManager mgr);
    IManager RetrieveInterface();
    Type GetManagerType();

    //state change?
    void PreStateChange(GameInstanceStates currentState);
    void OnStateChange(GameInstanceStates prevState,
            GameInstanceStates currState, GameInstanceStates nextState);
    void PostStateChange(GameInstanceStates nextState);
}
