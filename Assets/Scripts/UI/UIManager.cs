using System.Collections.Generic;
using UnityEngine;

namespace UIFramework
{

    public class UIManager : Singleton<UIManager>
    {
        private readonly Stack<IBasePanel> _panelStack = new();
        private Dictionary<string, IBasePanel> _uiPanelCache = new();     //已经实例化的面板字典

        public IReadOnlyCollection<IBasePanel> Panels => _panelStack;


        /// <summary>
        /// 为所有的面板统一执行Init初始化方法
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            var basePanels = GetComponentsInChildren<IBasePanel>(true);
            foreach (var panel in basePanels) panel.Init();
        }

        /// <summary>
        /// 展示初始化的面板
        /// </summary>
        private void Start()
        {
            GetUIPanel<TestPanel>().Show();
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                if (Peek() != null) Peek().OnPressedEsc();
        }


        #region 面板加载与缓存

        /// <summary>
        /// 获取或实例化面板，实际调用所有面板都依靠这个函数了
        /// </summary>
        public T GetUIPanel<T>() where T : BasePanel<T>
        {
            string panelName = typeof(T).Name;

            if (_uiPanelCache.TryGetValue(panelName, out var cachedPanel))
            {
                // 已存在，直接返回
                return cachedPanel as T;
            }

            if (!UIConst.UIPathDict.TryGetValue(panelName, out string path)) 
            {
                Debug.LogError($"未找到 {panelName} 的路径，请在 UIPathDict 中配置。");
                return null;
            }

            // 从 Resources 实例化
            GameObject prefab = Resources.Load<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogError($"未能在 {path} 找到UI预制体，请检查路径。");
                return null;
            }

            GameObject panelObj = Instantiate(prefab, transform);
            panelObj.name = panelName;

            var panel = panelObj.GetComponent<T>();
            if (panel == null)
            {
                Debug.LogError($"{panelName} 预制体上缺少 {typeof(T).Name} 脚本。");
                return null;
            }

            // 初始化并缓存
            panel.Init();
            panelObj.SetActive(false);
            _uiPanelCache.Add(panelName, panel);

            return panel;
        }

        #endregion

        #region Panel栈操作

        /// <summary>
        /// 查看栈顶元素但不删除
        /// </summary>
        /// <returns>返回栈顶Panel</returns>
        public IBasePanel Peek()
        {
            return _panelStack.Count <= 0 ? null : _panelStack.Peek();
        }

        /// <summary>
        /// 清理面板栈，会将栈内的全部面板执行Hide的同时执行gameObject.SetActive(false)
        /// </summary>
        public void ClearPanels()
        {
            int count = _panelStack.Count;
            for (int i = 0; i < count; i++)
            {
                var curPanel = Peek();
                curPanel.Hide();
                (curPanel as MonoBehaviour)?.gameObject.SetActive(false);
            }

            _panelStack.Clear();
        }

        /// <summary>
        /// 存入栈中，没有特殊情况建议不要使用。
        /// 因为面板基类的Show和Hide的默认实现已经对栈元素的进出进行了管理，一般直接调用Show和Hide即可。
        /// </summary>
        /// <param name="basePanel">要存入栈的面板</param>
        /// <param name="callback">是否先执行栈顶的渐渐隐藏</param>
        /// /// <example>例如[1]当2被存入后,变成了[1,2],此时callback参数决定1会不会调用CallBack(false)</example>
        public void PushPanel(IBasePanel basePanel, bool callback = true)
        {
            if (basePanel.IsInStack)
            {
                Debug.LogWarning("已经存在于栈内，无法再将其存入栈内");
                return;
            }

            if (callback && Peek() != null) Peek().CallBack(false);

            _panelStack.Push(basePanel);
            basePanel.CallBack(true);
        }

        /// <summary>
        /// 弹出栈顶元素，没有特殊情况建议不要使用。
        /// 因为面板基类的Show和Hide的默认实现已经对栈元素的进出进行了管理，一般直接调用Show和Hide即可。
        /// </summary>
        /// <param name="callback">弹出后，是否执行新的栈顶的渐渐显示</param>
        /// <example>例如[1,2]当2被弹出后,变成了[1],此时callback参数决定1会不会调用CallBack(true)</example>
        public IBasePanel PopPanel(bool callback = true)
        {
            if (_panelStack.Count <= 0)
            {
                Debug.LogError("栈为空,不能弹出");
                return null;
            }

            if (Peek() != null) Peek().CallBack(false);

            var res = _panelStack.Pop();
            if (callback && Peek() != null) Peek().CallBack(true);

            return res;
        }

        #endregion


        #region Debug

        [ContextMenu("PrintStack")]
        private void PrintStack()
        {
            Debug.Log("_panelStack还剩: " + _panelStack.Count + "个,分别是");
            foreach (var item in _panelStack)
            {
                Debug.Log(item);
            }
        }

        #endregion
    }
}

