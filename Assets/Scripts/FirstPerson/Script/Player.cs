using UnityEngine;

// Namespace para organização dos scripts do projeto
            namespace Runtime.Script
{
    // Script principal do jogador, responsável por inicializar e atualizar os componentes de personagem e câmera
    public class Player : MonoBehaviour
    {
        // Referência ao script que controla o personagem (movimentação, física, etc.)
        [SerializeField] private PlayerCharacter playerCharacter;
        // Referência ao script que controla a câmera do jogador
        [SerializeField] private PlayerCamera playerCamera;

        // Instância das ações de input do jogador (gerenciador de controles)
        private PlayerInputActions _inputActions;

        // Inicialização dos componentes e do sistema de input
        void Start()
        {
            // Trava o cursor no centro da tela (descomentando a linha)
            //Cursor.lockState = CursorLockMode.Locked;   

            // Cria e habilita o sistema de input
            _inputActions = new PlayerInputActions();
            _inputActions.Enable();

            // Inicializa o personagem e a câmera
            playerCharacter.Initialize();
            // Passa o alvo da câmera (normalmente a cabeça do personagem) para a câmera seguir
            playerCamera.Initialize(playerCharacter.GetCameraTarget());
        }

        // Libera recursos do sistema de input ao destruir o objeto
        void OnDestroy()
        {
            _inputActions.Dispose();
        }

        // Atualização a cada frame: processa input, atualiza rotação da câmera e movimentação do personagem
        void Update()
        {
            // Captura o grupo de inputs do gameplay
            var input = _inputActions.Gameplay;
            var deltaTime = Time.deltaTime;

            // Cria o input para a câmera (movimento do mouse)
            var cameraInput = new CameraInput { Look = input.Lock.ReadValue<Vector2>() };
            // Atualiza a rotação da câmera com base no input
            playerCamera.UpdateRotation(cameraInput);

            // Cria o input para o personagem (movimento, rotação, pulo, agachar)
            var characterInput = new CharacterInput
            {
                Rotation = playerCamera.transform.rotation, // Rotação da câmera para alinhar o personagem
                Move = input.Move.ReadValue<Vector2>(),     // Input de movimento (WASD)
                Jump = input.Jump.WasPressedThisFrame(),    // Input de pulo
                Crouch = input.Crouch.WasPressedThisFrame()
                    ? CrouchInput.Toggle                    // Alterna o estado de agachado
                    : CrouchInput.None                     // Nenhuma ação de agachar
            };
            // Atualiza o personagem com o input recebido
            playerCharacter.UpdateInput(characterInput);
            // Atualiza o corpo/física do personagem
            playerCharacter.UpdateBody(deltaTime);
        }

        // Atualização após o Update, usada para atualizar a posição da câmera seguindo o personagem
        void LateUpdate()
        {
            // Atualiza a posição da câmera para seguir o alvo do personagem (normalmente a cabeça)
            playerCamera.UpdatePosition(playerCharacter.GetCameraTarget());
        }
    }
}
