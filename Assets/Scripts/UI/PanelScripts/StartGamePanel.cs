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
        Debug.Log("��ʼ��Ϸ��弤��");
    }

    private void OnDisable()
    {
        Debug.Log("��ʼ��Ϸ�������");
    }
}
