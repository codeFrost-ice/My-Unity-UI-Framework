using System.Collections;
using System.Collections.Generic;
using UIFramework;
using UnityEngine;

public class SettingPanel : BasePanel<SettingPanel>
{
    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        Debug.Log("设置面板激活");
    }

    private void OnDisable()
    {
        Debug.Log("设置面板隐藏");
    }
}
