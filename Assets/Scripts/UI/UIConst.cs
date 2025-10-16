using System.Collections.Generic;

namespace UIFramework
{
    /// <summary>
    /// UI常量
    /// </summary>
    public static class UIConst
    {
        /// <summary>
        /// 一般的UI面板切换的间隔
        /// </summary>
        public const float UIDuration = 0.25f;

        /// <summary>
        /// 按钮冷却时间,一般给发送服务端请求的按钮使用,防止短时间发送过多请求
        /// </summary>
        public const float BtnClickCoolDown = 1f;

        /// <summary>
        /// UI配置路径字典
        /// </summary>
        public static readonly Dictionary<string, string> UIPathDict = new()
        {
            { "StartGamePanel", "Panels/StartGamePanel" },
            { "SettingPanel", "Panels/SettingPanel" },
            { "TestPanel", "Panels/TestPanel" }
        };
    }
}