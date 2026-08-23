using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Nebula
{
    public class GStateMachine
    {
        private Dictionary<int, GState> _states; // Map of stateId to state.

        private GState _currentState;
        private GState _idleState;

        // Allows outside sources to lock state transitions
        public bool TransitionLock = false;

        // Lock state transitions if a state was recently changed. This ensures that every state
        // will perform at least one action in a fixed update call.
        private bool _stateRecentlyChanged = false;

        public GStateMachine()
        {
            _states = new Dictionary<int, GState>();
        }

        public void AddState(GState newState)
        {
            _states.Add(newState.Id, newState);
        }

        public void SetIdleState(GState newState)
        {
            this.AddState(newState);
            _idleState = newState;
            _currentState = newState;
            _currentState.Enter(_currentState.Id);
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
            // Check all allowed transitions from the current state and pick the one with the highest priority.
            GState bestState = null;
            bool currentValid = _currentState.Condition();

            foreach (int stateId in _currentState.AllowedTransitions)
            {
                GState queryState = _states[stateId];
                if (queryState.Condition())
                {
                    // If the current state is valid, only higher priority states can interrupt.
                    // If the current state is invalid, pick the highest priority allowed transition.
                    if (!currentValid || queryState.Priority > _currentState.Priority)
                    {
                        if (bestState == null || queryState.Priority > bestState.Priority)
                        {
                            bestState = queryState;
                        }
                    }
                }
            }

            // Transition to the best valid state found.
            // If none of that is true, don't change states.
            if (bestState != null)
            {
                ChangeStateTo(bestState);
                return _currentState.Id;
            }

            // If the current state condition is no longer met, but no allowed transitions are active yet,
            // remain in the current state until a valid transition condition becomes true.
            if (!currentValid)
            {
                Debug.LogError($"[GStateMachine] Current state '{_currentState.Name}' (ID: {_currentState.Id}) condition is false, but no valid transition conditions are met. Staying in current state.");
            }

            return _currentState.Id;
        }

        public void ChangeStateTo(GState newState)
        {
            int fromStateId = _currentState.Id;
            _currentState.Leave();
            _currentState = newState;
            _currentState.Enter(fromStateId);
            _stateRecentlyChanged = true;
        }
    }
}
