using UnityEngine;

namespace Runtime.Script
{
    public class Player : MonoBehaviour
    {

        [SerializeField] private PlayerCharacter playerCharacter;
        [SerializeField] private PlayerCamera playerCamera;
    
        private PlayerInputActions _inputActions;
        void Start()
        {
            //Cursor.lockState = CursorLockMode.Locked;   
        
            _inputActions = new PlayerInputActions();
            _inputActions.Enable();
        
            playerCharacter.Initialize();
            playerCamera.Initialize(playerCharacter.GetCameraTarget());
        }

        void OnDestroy()
        {
            _inputActions.Dispose();
        }

        void Update()
        {
            var input = _inputActions.Gameplay;
            var deltaTime = Time.deltaTime;
            
            var cameraInput = new CameraInput{ Look = input.Lock.ReadValue<Vector2>() };
            playerCamera.UpdateRotation(cameraInput);

            var characterInput = new CharacterInput
            {
                Rotation = playerCamera.transform.rotation, 
                Move = input.Move.ReadValue<Vector2>(),
                Jump = input.Jump.WasPressedThisFrame(),
                Crouch = input.Crouch.WasPressedThisFrame()
                    ? CrouchInput.Toggle
                    : CrouchInput.None
            };
            playerCharacter.UpdateInput(characterInput);
            playerCharacter.UpdateBody(deltaTime);
        }

        void LateUpdate()
        {
            playerCamera.UpdatePosition(playerCharacter.GetCameraTarget());
        }
    }
}
