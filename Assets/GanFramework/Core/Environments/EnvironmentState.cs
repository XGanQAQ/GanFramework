namespace GanFramework.Core
{
    public class EnvironmentState
    {
        /// <summary>
        /// 自动检测：是否启用框架调试模式。对象池会启用调试信息、日志输出更详细、某些运行时检查更严格
        /// </summary>
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public const bool DebugMode = true;
#else
        public const bool DebugMode = false;
#endif
    }
}
