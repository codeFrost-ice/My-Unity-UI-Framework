using System.Collections;
using System.Collections.Generic;
using UIFramework;
using UnityEngine;

public class Test1Panel : BasePanel<Test1Panel>
{
    protected override void Awake()
    {
        base.Awake();
    }

    //private void OnEnable()
    //{
    //    Debug.Log("测试1面板激活");
    //}

    //private void OnDisable()
    //{
    //    Debug.Log("测试1面板隐藏");
    //}

    void Update()
    {
        //if(Input.GetKeyDown(KeyCode.W))
        //{
        //    UIManager.Instance.GetUIPanel<GamePlayPanel>(this).Show();
        //}
    }
}
