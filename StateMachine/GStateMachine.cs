using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Nebula
{
    public class GStateMachine
    {
        private List<GState> _states;

        private GState _currentState;
        private GState _idleState;

        // Allows outside sources to lock state transitions
        public bool TransitionLock = false;

        // Lock state transitions if a state was recently changed. This ensures that every state
        // will perform at least one action in a fixed update call.
        private bool _stateRecentlyChanged = false;

        public GStateMachine()
        {
            _states = new List<GState>();
        }

        public void AddState(GState newState)
        {
            _states.Add(newState);
        }

        public void SetIdleState(GState newState)
        {
            this.AddState(newState);
            _idleState = newState;
            _currentState = newState;
            _currentState.Enter();
        }

        public void PerformStateAction()
        {
            _currentState.FixedUpdate();
            _stateRecentlyChanged = false;
        }

        public int UpdateState()
        {
            _currentState.Update();

            // In case a we want to lock the states from changing.
            if (TransitionLock || _stateRecentlyChanged)
            {
                return _currentState.Id;
            }

            // Handle Transitions
            // Only check the states that are allowed transitions from the current state.
            foreach (int stateId in _currentState.AllowedTransitions)
            {
                GState queryState = _states[stateId];
                if (queryState.Condition())
                {
                    if (!_currentState.Condition())
                    {
                        ChangeStateTo(queryState);
                    }
                    else if (queryState.Priority > _currentState.Priority)
                    {
                        // Only use priorities if both state conditions are true.
                        ChangeStateTo(queryState);
                    }
                }
            }

            // Finally, if no transition state condition is met and the current state condition
            // isn't met set the current state to the idle state.
            // (Make this gradually go back using a graph at somepoint)
            if (!_currentState.Condition())
            {
                ChangeStateTo(_idleState); // More of a fail safe currently.
            }

            return _currentState.Id;
        }

        public void ChangeStateTo(GState newState)
        {
            _currentState.Leave();
            _currentState = newState;
            _currentState.Enter();
            _stateRecentlyChanged = true;
        }
    }
}
