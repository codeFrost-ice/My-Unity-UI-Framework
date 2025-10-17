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
            //GetUIPanel<TestPanel>().Show();
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                GetUIPanel<Test1Panel>().Show();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                GetUIPanel<TestPanel>().Show();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
                if (Peek() != null) Peek().OnPressedEsc();
        }


        #region 面板加载与缓存

        /// <summary>
        /// 获取或实例化面板，实际调用所有面板都依靠这个函数了
        /// </summary>
        public T GetUIPanel<T>(IBasePanel parentPanel = null) where T : BasePanel<T>
        {
            string panelName = typeof(T).Name;

            if (_uiPanelCache.TryGetValue(panelName, out var cachedPanel))
            {
                //Debug.Log($"GetUIPanel()    缓存找到 {panelName} ，激活。");
                return cachedPanel as T;
            }

            if (!UIConst.UIPathDict.TryGetValue(panelName, out string path)) 
            {
                Debug.LogError($"GetUIPanel()   未找到 {panelName} 的路径，请在 UIPathDict 中配置。");
                return null;
            }

            // 从 Resources 实例化
            GameObject prefab = Resources.Load<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogError($"GetUIPanel()   未能在 {path} 找到UI预制体，请检查路径。");
                return null;
            }

            GameObject panelObj = Instantiate(prefab, transform);
            panelObj.name = panelName;

            var panel = panelObj.GetComponent<T>();
            if (panel == null)
            {
                Debug.LogError($"GetUIPanel()   {panelName} 预制体上缺少 {typeof(T).Name} 脚本。");
                return null;
            }

            // 初始化并缓存
            panel.Init();
            panelObj.SetActive(false);
            _uiPanelCache.Add(panelName, panel);

            if (parentPanel != null)
                panel.SetParentPanel(parentPanel);

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
                Debug.LogWarning("PushPanel()   已经存在于栈内，无法再将其存入栈内");
                return;
            }

            var topPanel = Peek();
            // 情况1：当前没有面板，直接入栈
            if (topPanel == null)
            {
                _panelStack.Push(basePanel);
                basePanel.CallBack(true);
                return;
            }

            // 2️⃣ 如果当前栈顶是“父面板”
            if (topPanel is IBasePanel parentPanel && parentPanel != null)
            {
                // 2.1 当前入栈面板的父面板与栈顶相同 => 属于子面板，不隐藏父面板
                if (basePanel.ParentPanel == parentPanel)
                {
                    //Debug.Log($"PushPanel() 子面板 {basePanel} 属于父面板 {parentPanel}，不隐藏父面板");
                }
                else
                {
                    // 2.2 不同父面板 => 隐藏整个旧父面板分组
                    //Debug.Log($"PushPanel() 新面板 {basePanel} 不属于当前父面板 {parentPanel}，隐藏旧面板组");
                    HidePanelGroup(parentPanel);
                }
            }

            // 3️⃣ 如果当前入栈的面板本身是父面板
            if (basePanel.ParentPanel == null)
            {
                // 隐藏所有不属于它的旧面板组（不出栈，只callback(false)）
                HideAllExceptParent(basePanel);
            }

            // 4️⃣ 入栈
            _panelStack.Push(basePanel);
            basePanel.CallBack(true);

            //if (callback && Peek() != null && !Peek().IsParentPanel) Peek().CallBack(false);
            //_panelStack.Push(basePanel);
            //basePanel.CallBack(true);
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
                Debug.LogError("PopPanel()  栈为空,不能弹出");
                return null;
            }

            var popped = _panelStack.Pop();
            popped.CallBack(false);     // 弹出的面板渐隐

            if (callback && _panelStack.Count > 0)
            {
                var newTop = Peek();

                // 如果新栈顶有父面板，让它和它的子面板渐显
                if (newTop.ParentPanel != null)
                {
                    var parentPanel = newTop.ParentPanel;
                    parentPanel.CallBack(true);
                    foreach (var p in _panelStack)
                    {
                        if (p.ParentPanel == parentPanel)
                            p.CallBack(true);
                        break;  //找到第一个就可以，不能破坏栈层级结构
                    }
                }
                else
                {
                    newTop.CallBack(true);
                }
            }

            return popped;
        }

        /// <summary>
        /// 隐藏某个父面板及其所有子面板（不移出栈）
        /// </summary>
        private void HidePanelGroup(IBasePanel parent)
        {
            foreach (var panel in _panelStack)
            {
                if (panel == parent || panel.ParentPanel == parent)
                {
                    panel.CallBack(false);
                }
            }
        }

        /// <summary>
        /// 隐藏所有与指定父面板无关的面板组（但不Pop）
        /// </summary>
        private void HideAllExceptParent(IBasePanel currentParent)
        {
            foreach (var panel in _panelStack)
            {
                if (panel == currentParent) continue;
                if (panel.ParentPanel != currentParent)
                {
                    panel.CallBack(false);
                }
            }
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

