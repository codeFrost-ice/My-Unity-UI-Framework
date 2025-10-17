using System.Collections;
using System.Collections.Generic;
using UIFramework;
using UnityEngine;

public class StartGamePanel : BasePanel<StartGamePanel>
{
    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        Debug.Log("开始游戏面板激活");
    }

    private void OnDisable()
    {
        Debug.Log("开始游戏面板隐藏");
    }
}
