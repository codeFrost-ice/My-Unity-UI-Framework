using System.Collections;
using System.Collections.Generic;
using UIFramework;
using UnityEngine;

public class TestPanel : BasePanel<TestPanel>
{
    protected override void Awake()
    {
        base.Awake();
    }

    //private void OnEnable()
    //{
    //    Debug.Log("测试面板激活");
    //}

    //private void OnDisable()
    //{
    //    Debug.Log("测试面板隐藏");
    //}

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            UIManager.Instance.GetUIPanel<SettingPanel>(this).Show();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            UIManager.Instance.GetUIPanel<GamePlayPanel>(this).Show();
        }
    }
}
