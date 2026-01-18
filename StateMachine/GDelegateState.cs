using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nebula
{
    public class GDelegateState : GState
    {
        // Callback functions so that any general 'player controller' can create states
        // w/o having to change or add dependencies to itself or a GState.
        public delegate bool DelegatedCondition();
        private DelegatedCondition _condition;

        public delegate void DelegatedAction();
        private DelegatedAction _action;

        public delegate void DelegatedEnter(int fromStateId);
        private DelegatedEnter _enter;

        public delegate void DelegatedLeave();
        private DelegatedLeave _leave;

        public delegate void DelegatedUpdate();
        private DelegatedUpdate _update;
        // Needs state ID, Condition, List of allowed transitions, priority, name
        public GDelegateState(int id,
                              int priority,
                              List<int> allowedTransitions,
                              DelegatedCondition condition,
                              string name = "N/A",
                              DelegatedAction action = null,
                              DelegatedEnter enter = null,
                              DelegatedLeave leave = null,
                              DelegatedUpdate update = null,
                              Animator animator = null,
                              string animatorBoolName = "N/A")
                              :
                              base(id, priority, allowedTransitions, name, animator, animatorBoolName)
        {
            _condition = condition;
            _action = action;
            _enter = enter;
            _leave = leave;
            _update = update;
        }

        public override void Enter(int fromStateId)
        {
            base.Enter(fromStateId);
            if (_enter == null) return; // Saftey Check. Not every component needs to be implemented.
            _enter(fromStateId);
        }

        public override void Leave()
        {
            base.Leave();
            if (_leave == null) return; // Saftey Check. Not every component needs to be implemented.
            _leave();
        }

        public override bool Condition()
        {
            return _condition();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (_action == null) return; // Saftey Check. Not every component needs to be implemented.
            _action();
        }

        public override void Update()
        {
            base.Update();
            if (_update == null) return; // Saftey Check. Not every component needs to be implemented.
            _update();
        }
    }
}