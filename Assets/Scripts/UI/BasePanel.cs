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
        void Hide();

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

        protected override void Awake()
        {
            base.Awake();
        }

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
            UIManager.Instance.PushPanel(this);
            gameObject.SetActive(true);
            //设置为最后一个子物体，防止被其他已经打开的面板遮挡
            gameObject.transform.SetAsLastSibling();
            IsInStack = true;
        }

        /// <summary>
        /// 关闭当前面板，IsInStack = false,一般只对栈顶的元素执行,若不是栈顶元素执行,将会弹出该元素之上的所有元素和他自己   
        ///  TODO: 要改为区分小的子Panel，判断是否是需要存栈的Panel
        /// </summary>
        public void Hide()
        {
            if (!IsInStack) return;

            if (ReferenceEquals(UIManager.Instance.Peek(), this))
            {
                UIManager.Instance.PopPanel();
                IsInStack = false;
            }
            else
            {
                Debug.LogWarning("注意，你关闭了栈顶以外的面板");
                while (!ReferenceEquals(UIManager.Instance.Peek(), this))
                {
                    UIManager.Instance.Peek().Hide();
                    UIManager.Instance.PopPanel();
                    IsInStack = false;
                }
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
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected T GetControl<T>(string controlName) where T : UIBehaviour
        {
            if (!_controlDic.ContainsKey(controlName)) return null;
            for (var i = 0; i < _controlDic[controlName].Count; ++i)
                if (_controlDic[controlName][i] is T)
                    return _controlDic[controlName][i] as T;

            return null;
        }

        private void FindChildrenControl<T>() where T : UIBehaviour
        {
            var controls = GetComponentsInChildren<T>(true);
            for (var i = 0; i < controls.Length; ++i)
            {
                var objName = controls[i].gameObject.name;
                if (_controlDic.TryGetValue(objName, out var value1))
                    value1.Add(controls[i]);
                else
                    _controlDic.Add(objName, new List<UIBehaviour> { controls[i] });

                if (controls[i] is Button)
                    (controls[i] as Button)?.onClick.AddListener(() => { OnClick(objName); });
                else if (controls[i] is Toggle)
                    (controls[i] as Toggle)?.onValueChanged.AddListener(value => { OnValueChanged(objName, value); });
            }
        }
    }
}