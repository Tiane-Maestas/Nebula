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

        public delegate void DelegatedEnter();
        private DelegatedEnter _enter;

        public delegate void DelegatedLeave();
        private DelegatedLeave _leave;

        public delegate void DelegatedUpdate();
        private DelegatedUpdate _update;

        public GDelegateState(DelegatedCondition condition, DelegatedAction action,
                              DelegatedEnter enter, DelegatedLeave leave,
                              DelegatedUpdate update,
                              int id, string name, List<int> allowedTransitions,
                              int priority, Animator animator, string animatorBoolName)
                              :
                              base(id, name, allowedTransitions, priority, animator,
                              animatorBoolName)
        {
            _condition = condition;
            _action = action;
            _enter = enter;
            _leave = leave;
            _update = update;
        }

        public override void Enter()
        {
            base.Enter();
            // Saftey Check. Not every component needs to be implemented.
            if (_enter == null)
            {
                return;
            }
            _enter();
        }

        public override void Leave()
        {
            base.Leave();
            // Saftey Check. Not every component needs to be implemented.
            if (_leave == null)
            {
                return;
            }
            _leave();
        }

        public override bool Condition()
        {
            return _condition();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            // Saftey Check. Not every component needs to be implemented.
            if (_action == null)
            {
                return;
            }
            _action();
        }

        public override void Update()
        {
            base.Update();
            // Saftey Check. Not every component needs to be implemented.
            if (_update == null)
            {
                return;
            }
            _update();
        }
    }
}