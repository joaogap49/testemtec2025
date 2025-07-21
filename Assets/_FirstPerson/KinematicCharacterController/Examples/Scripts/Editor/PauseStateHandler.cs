﻿using KinematicCharacterController.Core;
using UnityEditor;
using UnityEngine;

namespace KinematicCharacterController.Examples.Scripts.Editor
{
    public class PauseStateHandler
    {
        [RuntimeInitializeOnLoadMethod()]
        public static void Init()
        {
            EditorApplication.pauseStateChanged += HandlePauseStateChange;
        }

        private static void HandlePauseStateChange(PauseState state)
        {
            foreach(KinematicCharacterMotor motor in KinematicCharacterSystem.CharacterMotors)
            {
                motor.SetPositionAndRotation(motor.Transform.position, motor.Transform.rotation, true);
            }
        }
    }
}
