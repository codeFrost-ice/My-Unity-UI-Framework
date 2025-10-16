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

    private void OnEnable()
    {
        Debug.Log("������弤��");
    }

    private void OnDisable()
    {
        Debug.Log("�����������");
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            UIManager.Instance.GetUIPanel<SettingPanel>().Show();
        }
    }
}
