﻿using KinematicCharacterController.Core;
using UnityEngine;
using UnityEngine.Playables;

namespace KinematicCharacterController.Walkthrough._8__Creating_a_moving_platform.Scripts
{
    public struct MyMovingPlatformState
    {
        public PhysicsMoverState MoverState;
        public float DirectorTime;
    }

    public class MyMovingPlatform : MonoBehaviour, IMoverController
    {
        public PhysicsMover Mover;

        public PlayableDirector Director;

        private Transform _transform;

        private void Start()
        {
            _transform = this.transform;

            Mover.MoverController = this;
        }

        // This is called every FixedUpdate by our PhysicsMover in order to tell it what pose it should go to
        public void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
        {
            // Remember pose before animation
            Vector3 _positionBeforeAnim = _transform.position;
            Quaternion _rotationBeforeAnim = _transform.rotation;

            // Update animation
            EvaluateAtTime(Time.time);

            // Set our platform's goal pose to the animation's
            goalPosition = _transform.position;
            goalRotation = _transform.rotation;

            // Reset the actual transform pose to where it was before evaluating. 
            // This is so that the real movement can be handled by the physics mover; not the animation
            _transform.position = _positionBeforeAnim;
            _transform.rotation = _rotationBeforeAnim;
        }

        public void EvaluateAtTime(double time)
        {
            Director.time = time % Director.duration;
            Director.Evaluate();
        }
    }
}