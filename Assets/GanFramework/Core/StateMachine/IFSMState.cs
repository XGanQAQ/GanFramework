namespace Core
{
    public interface IFSMState
    {
        /// <summary>
        /// 进入该状态时执行的
        /// </summary>
        void Enter();

        /// <summary>
        /// 相当于用Unity生命周期中的Update，用于逻辑更新
        /// </summary>
        void LogicalUpdate();

        /// <summary>
        /// 状态结束时（即转移出时）执行的
        /// </summary>
        void Exit();
    }
}