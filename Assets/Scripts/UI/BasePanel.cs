using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIFramework
{
    /// <summary>
    /// 面板接口
    /// </summary>
    public interface IBasePanel
    {
        /// <summary>
        ///     存父亲面板
        /// </summary>
        IBasePanel ParentPanel { get; }

        /// <summary>
        ///     是否存在于面板栈内
        /// </summary>
        bool IsInStack { get; }

        /// <summary>
        ///     初始化方法
        /// </summary>
        void Init();

        /// <summary>
        ///     Show的同时会将其存入栈内
        /// </summary>
        void Show();

        /// <summary>
        ///     Hide的同时会尝试将其弹出，注意不能调用栈顶以外面板的Hide
        /// </summary>
        void Hide(bool isParentCallBack = false);

        /// <summary>
        ///     按下ESC键时触发
        /// </summary>
        void OnPressedEsc();

        /// <summary>
        ///     根据IsInStack来决定执行Show还是Hide
        /// </summary>
        void Change();

        /// <summary>
        ///     true表示该面板作为新的元素入栈顶时的标记（可以写渐渐出现的逻辑），false时表示新的元素入栈后原来的栈顶（this）的标记（可以写消失的逻辑）
        /// </summary>
        /// <param name="flag">true表示为栈顶，flag表示有新的元素替代了原来的栈顶</param>
        /// <example>例如栈为[1],新push了一个2变为[1,2],此时1会执行CallBack(false),2会执行CallBack(true)</example>
        void CallBack(bool flag);
    }


    /// <summary>
    /// 面板基类
    /// </summary>
    /// <typeparam name="T">决定单例模式Instance的类型</typeparam>
    public class BasePanel<T> : Singleton<T>, IBasePanel where T : BasePanel<T>
    {
        [SerializeField] private bool isParentPanel = false;
        private readonly Dictionary<string, List<UIBehaviour>> _controlDic = new();
        private CanvasGroup _canvasGroup;

        /// <summary>
        ///     CanvasGroup的实例对象，访问时如果没有则会自动创建
        /// </summary>
        protected CanvasGroup CanvasGroupInstance
        {
            get
            {
                _canvasGroup ??= gameObject.AddComponent<CanvasGroup>();
                return _canvasGroup;
            }
        }

        /// <summary>
        /// 是否在栈中
        /// </summary>
        public bool IsInStack { get; private set; }

        public IBasePanel ParentPanel { get; private set; }

        // 允许 UIManager 在创建时赋值
        public void SetParentPanel(IBasePanel parent)
        {
            ParentPanel = parent;
        }

        /// <summary>
        /// 面板是否显示
        /// </summary>
        public bool IsVisible { get; private set; } = false;

        /// <summary>
        /// 初始化，搜寻控件
        /// </summary>
        public virtual void Init()
        {
            //添加控件
            FindChildrenControl<Button>();
            FindChildrenControl<Image>();
            FindChildrenControl<Text>();
            FindChildrenControl<TextMeshProUGUI>();
            FindChildrenControl<TMP_InputField>();
            FindChildrenControl<Toggle>();
            FindChildrenControl<ToggleGroup>();
            FindChildrenControl<Slider>();
            FindChildrenControl<ScrollRect>();
            FindChildrenControl<InputField>();
        }

        /// <summary>
        /// 打开当前面板，该面板进入栈内，同时标记为IsInStack = true    TODO: 要改为第一次查找创建，然后再变为Active变化
        /// </summary>
        public void Show()
        {
            if (IsInStack) return;

            if (!gameObject.activeSelf) gameObject.SetActive(true);

            UIManager.Instance.PushPanel(this, false);
            transform.SetAsLastSibling();       //设置为最后一个子物体，防止被其他已经打开的面板遮挡
            IsInStack = true;
        }

        /// <summary>
        /// 关闭当前面板，IsInStack = false,一般只对栈顶的元素执行,若不是栈顶元素执行,将会弹出该元素之上的所有元素和他自己   
        ///  TODO: 要改为区分小的子Panel，判断是否是需要存栈的Panel
        /// </summary>
        public void Hide(bool isParentCallBack = false)
        {
            if (!IsInStack) return;

            while (true)
            {
                var topPanel = UIManager.Instance.Peek();

                if (topPanel == null)
                {
                    Debug.LogWarning("Hide()    栈已空，停止Hide循环");
                    break;
                }

                // 如果相等，直接关闭结束
                if (ReferenceEquals(topPanel, this))
                {
                    UIManager.Instance.PopPanel(!isParentCallBack);
                    IsInStack = false;
                    break;
                }

                //Debug.Log("Hide()   不满足条件：" + topPanel + " This：" + this);
                // 如果顶部刚好是关闭的子面板
                if(ReferenceEquals(topPanel.ParentPanel, this))
                {
                    topPanel.Hide(true); // 隐藏当前栈顶，加上父亲callback不要再出现
                }
                topPanel.Hide();
            }
        }

        public void Change()
        {
            if (IsInStack) Hide();
            else Show();
        }

        public void OnPressedEsc()
        {
            Hide();
        }

        /// <summary>
        /// true表示该面板作为新的元素入栈顶时的标记（可以写渐渐出现的逻辑），false时表示新的元素入栈后原来的栈顶（this）的标记（可以写消失的逻辑）
        /// </summary>
        /// <param name="flag">true表示为栈顶，flag表示有新的元素替代了原来的栈顶</param>
        /// <example>例如栈为[1],新push了一个2变为[1,2],此时1会执行CallBack(false),2会执行CallBack(true)</example>
        public void CallBack(bool flag)
        {
            if (flag && IsVisible) return;   // 防止重复渐显
            if (!flag && !IsVisible) return; // 防止重复渐隐

            IsVisible = flag;
            ChangePanelAlphaEffect(flag);
        }

        #region Panel切换效果

        /// <summary>
        /// 伸缩
        /// </summary>
        /// <param name="flag"></param>
        private void ChangePanelScaleEffect(bool flag)
        {
            transform.DOKill(true);
            if (flag)
            {
                CanvasGroupInstance.interactable = true;
                gameObject.SetActive(true);
                transform.localScale = Vector3.zero;
                transform.DOScale(1, UIConst.UIDuration);
            }
            else
            {
                CanvasGroupInstance.interactable = false;
                transform.DOScale(0, UIConst.UIDuration).OnComplete(() => { gameObject.SetActive(false); });
            }
        }

        /// <summary>
        /// 淡入淡出
        /// </summary>
        /// <param name="flag"></param>
        private void ChangePanelAlphaEffect(bool flag)
        {
            transform.DOKill(true);
            CanvasGroupInstance.DOKill(true);
            if (flag)
            {
                CanvasGroupInstance.interactable = true;
                gameObject.SetActive(true);
                CanvasGroupInstance.alpha = 0f;
                CanvasGroupInstance.DOFade(1, UIConst.UIDuration);
            }
            else
            {
                CanvasGroupInstance.interactable = false;
                //CanvasGroupInstance.blocksRaycasts = false; // 渐隐时通常不希望接收 UI 输入
                CanvasGroupInstance.DOFade(0, UIConst.UIDuration).OnComplete(() => { gameObject.SetActive(false); });
            }
        }

        #endregion

        /// <summary>
        /// 所有按钮的点击事件，可以考虑在这里添加音效
        /// </summary>
        /// <param name="btnName">这个按钮的GameObject的名称</param>
        protected virtual void OnClick(string btnName)
        {

        }

        /// <summary>
        /// 所有多选框的点击事件，可以考虑在这里添加音效
        /// </summary>
        /// <param name="toggleName">这个多选框的GameObject的名称</param>
        /// <param name="value">多选框回调返回值</param>
        protected virtual void OnValueChanged(string toggleName, bool value)
        {

        }

        /// <summary>
        /// 根据在场景中GameObject的名称来寻找UI控件，如果同名则返回第一个找到的（从上往下）
        /// </summary>
        /// <param name="controlName"></param>
        /// <typeparam name="0"></typeparam>
        /// <returns></returns>
        protected T0 GetControl<T0>(string controlName) where T0 : UIBehaviour
        {
            if (!_controlDic.ContainsKey(controlName)) return null;

            for (var i = 0; i < _controlDic[controlName].Count; ++i)
            {
                if (_controlDic[controlName][i] is T0)
                {
                    return _controlDic[controlName][i] as T0;
                }
            }

            return null;
        }

        private void FindChildrenControl<T1>() where T1 : UIBehaviour
        {
            var controls = GetComponentsInChildren<T1>(true);
            for (var i = 0; i < controls.Length; ++i)
            {
                var objName = controls[i].gameObject.name;

                if (_controlDic.TryGetValue(objName, out var value1))
                {
                    value1.Add(controls[i]);
                }
                else
                {
                    _controlDic.Add(objName, new List<UIBehaviour> { controls[i] });
                }

                if (controls[i] is Button)
                {
                    (controls[i] as Button)?.onClick.AddListener(() => { OnClick(objName); });
                }
                else if (controls[i] is Toggle)
                {
                    (controls[i] as Toggle)?.onValueChanged.AddListener(value => { OnValueChanged(objName, value); });
                }
            }
        }
    }
}