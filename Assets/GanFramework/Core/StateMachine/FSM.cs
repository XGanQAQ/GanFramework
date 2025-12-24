using System;
using System.Collections.Generic;
namespace Core
{
    public class FSM<T> where T : IFSMState
    {
        public Dictionary<System.Type, T> StateTable{ get; protected set; } //状态表
        protected T curState; //当前状态
        public Action<System.Type> OnChangeState;

        public FSM()
        {
            StateTable = new Dictionary<System.Type, T>();
            curState = default;
        }

        public void AddState(T state)
        {
            StateTable.Add(state.GetType(), state);
        }

        //设置状态机的第一个状态时使用，因为一开始的curState还是空的
        //故不需要 curState.Exit()
        public void SwitchOn(System.Type startState)
        {
            curState = StateTable[startState];
            curState.Enter();
        }

        public void ChangeState(System.Type nextState)
        {
            curState.Exit();
            curState = StateTable[nextState];
            curState.Enter();
            OnChangeState?.Invoke(nextState);
        }

        public void OnUpdate()
        {
            curState.LogicalUpdate();
        }
    }
}