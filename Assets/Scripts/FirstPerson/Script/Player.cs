using UnityEngine;

// Namespace para organiza��o dos scripts do projeto
            namespace Runtime.Script
{
    // Script principal do jogador, respons�vel por inicializar e atualizar os componentes de personagem e c�mera
    public class Player : MonoBehaviour
    {
        // Refer�ncia ao script que controla o personagem (movimenta��o, f�sica, etc.)
        [SerializeField] private PlayerCharacter playerCharacter;
        // Refer�ncia ao script que controla a c�mera do jogador
        [SerializeField] private PlayerCamera playerCamera;

        // Inst�ncia das a��es de input do jogador (gerenciador de controles)
        private PlayerInputActions _inputActions;

        // Inicializa��o dos componentes e do sistema de input
        void Start()
        {
            // Trava o cursor no centro da tela (descomentando a linha)
            //Cursor.lockState = CursorLockMode.Locked;   

            // Cria e habilita o sistema de input
            _inputActions = new PlayerInputActions();
            _inputActions.Enable();

            // Inicializa o personagem e a c�mera
            playerCharacter.Initialize();
            // Passa o alvo da c�mera (normalmente a cabe�a do personagem) para a c�mera seguir
            playerCamera.Initialize(playerCharacter.GetCameraTarget());
        }

        // Libera recursos do sistema de input ao destruir o objeto
        void OnDestroy()
        {
            _inputActions.Dispose();
        }

        // Atualiza��o a cada frame: processa input, atualiza rota��o da c�mera e movimenta��o do personagem
        void Update()
        {
            // Captura o grupo de inputs do gameplay
            var input = _inputActions.Gameplay;
            var deltaTime = Time.deltaTime;

            // Cria o input para a c�mera (movimento do mouse)
            var cameraInput = new CameraInput { Look = input.Lock.ReadValue<Vector2>() };
            // Atualiza a rota��o da c�mera com base no input
            playerCamera.UpdateRotation(cameraInput);

            // Cria o input para o personagem (movimento, rota��o, pulo, agachar)
            var characterInput = new CharacterInput
            {
                Rotation = playerCamera.transform.rotation, // Rota��o da c�mera para alinhar o personagem
                Move = input.Move.ReadValue<Vector2>(),     // Input de movimento (WASD)
                Jump = input.Jump.WasPressedThisFrame(),    // Input de pulo
                Crouch = input.Crouch.WasPressedThisFrame()
                    ? CrouchInput.Toggle                    // Alterna o estado de agachado
                    : CrouchInput.None                     // Nenhuma a��o de agachar
            };
            // Atualiza o personagem com o input recebido
            playerCharacter.UpdateInput(characterInput);
            // Atualiza o corpo/f�sica do personagem
            playerCharacter.UpdateBody(deltaTime);
        }

        // Atualiza��o ap�s o Update, usada para atualizar a posi��o da c�mera seguindo o personagem
        void LateUpdate()
        {
            // Atualiza a posi��o da c�mera para seguir o alvo do personagem (normalmente a cabe�a)
            playerCamera.UpdatePosition(playerCharacter.GetCameraTarget());
        }
    }
}
