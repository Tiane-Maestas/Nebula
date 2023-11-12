using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nebula
{
    public class GState
    {
        // Identifiers for the state machine and human's.
        public int Id { get; private set; }
        public string Name { get; private set; }

        // Allows for toggling of animations to play.
        private string _animatorBoolName;
        private Animator _animator;

        // The time when the 'enter' method was last called.
        public float StartTime { get; private set; }

        // A list of 'id' to specify what transitions are allowed.
        public List<int> AllowedTransitions { get; private set; }

        // In the case that multiple 'transitions' are allowed choose the one with
        // the highest priority.
        public int Priority { get; private set; }

        public GState(int id, int priority, List<int> allowedTransitions, string name,
                      Animator animator, string animatorBoolName)
        {
            this.Id = id;
            this.Name = name;
            this.AllowedTransitions = allowedTransitions;
            this.Priority = priority;
            _animator = animator;
            _animatorBoolName = animatorBoolName;
        }

        public virtual void Enter()
        {
            StartTime = Time.time;
            if (_animator != null)
                _animator.SetBool(_animatorBoolName, true);
            // ToDo: Implement the logic that is performed once when entering a state
        }

        public virtual void Leave()
        {
            if (_animator != null)
                _animator.SetBool(_animatorBoolName, false);
            // ToDo: Implement the logic that is performed once when leaving a state
        }

        public virtual bool Condition()
        {
            // ToDo: Implement what condition is required to be in this state.
            return false;
        }

        public virtual void FixedUpdate()
        {
            // ToDo: Implement the action that this state performs
        }

        public virtual void Update()
        {
            // ToDo: Implement logic that this state needs to be checked every frame.
        }

        public override string ToString() { return this.Name; }
    }
}
