using System.Numerics;
using KinematicCharacterController.Core;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Runtime.Script
{

    public enum CrouchInput
    {
        None, Toggle
    }

    public enum Stance
    {
        Stand, Crouch
    }
    public struct CharacterInput
    {
        public Quaternion Rotation;
        public Vector2 Move;
        public bool Jump;
        public CrouchInput Crouch;
    }

    public class PlayerCharacter : MonoBehaviour, ICharacterController
    {
        
        [SerializeField] private KinematicCharacterMotor motor;
        [SerializeField] private Transform root;
        [SerializeField] private Transform cameraTarget;
        [Space]
        [SerializeField] private float crouchSpeed = 7f;
        [SerializeField] private float walkSpeed = 20f;
        [SerializeField] private float walkResponse = 25f;
        [SerializeField] private float crouchResponse = 20f;
        [Space]
        [SerializeField] private float jumpSpeed = 20f;
        [SerializeField] private float gravity = -90f;
        [Space]
        [SerializeField] private float standHeight = 2f;
        [SerializeField] private float crouchHeight = 1f;
        [SerializeField] private float crouchHeightResponse = 15f;
        [Range(0f, 1f)]
        [SerializeField] private float standCameraTargetHeight = 0.9f;
        [Range(0f, 1f)]
        [SerializeField] private float crouchCameraTargetHeight = 0.7f;
        
        private Stance _stance;
        
        private Quaternion _requestedRotation;
        private Vector3 _requestedMovement;
        private bool _requestedJump;
        private bool _requestedCrouch;

        private Collider[] _uncrouchOvelapResults;
        
        public void Initialize()
        {
            _stance = Stance.Stand;
            _uncrouchOvelapResults = new Collider[8];
            
            motor.CharacterController = this;
        }

        public void UpdateInput(CharacterInput input)
        {
            _requestedRotation = input.Rotation;
            // Pega o vetor de 'input' 2D e cria um vetor de movimento 3D no plano XZ.
            _requestedMovement = new Vector3(input.Move.x, 0, input.Move.y);
            // Coloca o comprimento para ser 1 para prevenir do 'player' se mexer mais rápido diagonalmente (O mesmo do 'Normalized')
            _requestedMovement = Vector3.ClampMagnitude(_requestedMovement, 1f);
            // Orientar o 'input' para ser relativo à direção que o 'player' está a olhando.
            _requestedMovement = input.Rotation * _requestedMovement;
        
            _requestedJump = _requestedJump || input.Jump;
            _requestedCrouch = input.Crouch switch
            {
                CrouchInput.Toggle => !_requestedCrouch,
                CrouchInput.None => _requestedCrouch,
                _ => _requestedCrouch

            };
        }

        public void UpdateBody(float deltaTime)
        {
            var currentHeight = motor.Capsule.height;
            var normalizedHeight = currentHeight / standHeight;
            var cameraTargetHeight = currentHeight *
            (
                  _stance is Stance.Stand
                  ? standCameraTargetHeight
                  : crouchCameraTargetHeight
            );
            var rootTargetScale = new Vector3(1f, normalizedHeight, 1f);
            
            cameraTarget.localPosition = Vector3.Lerp(
                a: cameraTarget.localPosition,
                b: new Vector3(0f, cameraTargetHeight, 0f),
                t: 1f - Mathf.Exp(-crouchHeightResponse * deltaTime)
            );
            root.localScale = Vector3.Lerp(
                a: root.localScale,
                b: rootTargetScale,
                t: 1f - Mathf.Exp(-crouchHeightResponse * deltaTime)
            );
        }
    
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            // Atualizar a rotação do personagem para a mesma direção da rotação requisitada (camera rotation).
        
            // Não queremos que o personagem ande para cima ou para baixo (parecendo No Clip)
            // então a direção do personagem tem que ser sempre 'flat' ou "plana".
        
            // Então projetaremos para que o vetor aponte sempre para a mesma direção que o "player" está a olhar
            // num chão plano.
        
            var forward = Vector3.ProjectOnPlane
            (
                _requestedRotation * Vector3.forward, motor.CharacterUp
            );
        
            if (forward != Vector3.zero){
                currentRotation = Quaternion.LookRotation(forward, motor.CharacterUp);
            }
        }
    

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            // Se estiver no chão...
            if (motor.GroundingStatus.IsStableOnGround)
            {
                var groundedMovement = motor.GetDirectionTangentToSurface
                (
                    direction: _requestedMovement,
                    surfaceNormal: motor.GroundingStatus.GroundNormal
                ) * _requestedMovement.magnitude;
                
                // calcula a velocidade e resposta do movimento
                // baseado na posição do personagem
                var speed = _stance is Stance.Stand
                    ? walkSpeed
                    : crouchSpeed;
                
                var response = _stance is Stance.Stand
                    ?walkResponse
                    : crouchResponse;

                var targetVelocity = groundedMovement * speed;
                currentVelocity = Vector3.Lerp(
                    a: currentVelocity,
                    b: targetVelocity,
                    t: 1f - Mathf.Exp(-response * deltaTime)
                );
            }
        
            // Senão, no ar...
            else
            {
                // Gravidade
                currentVelocity += motor.CharacterUp * (gravity * deltaTime);
            }
            if (_requestedJump)
            {
                _requestedJump = false;
            
                // Tira o 'player' do chão.
                motor.ForceUnground();
            
                // Define uma velocidade minima verticalmente para a velocidade do pulo.
                var currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
                var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);
                // Adiciona a diferença na atual e na desejada velocidade vertical do 'player'.
                currentVelocity += motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
            }

        }

        public void BeforeCharacterUpdate(float deltaTime)
        {
            // Crouch
            if (_requestedCrouch && _stance is Stance.Stand)
            {
                _stance = Stance.Crouch;
                motor.SetCapsuleDimensions
                (
                    radius: motor.Capsule.radius,
                    height: crouchHeight,
                    yOffset: crouchHeight * 0.5f
                );
            }
        }

        public void PostGroundingUpdate(float deltaTime)
        {

        }

        public void AfterCharacterUpdate(float deltaTime)
        {
            // Uncrouch
            if (!_requestedCrouch && _stance is not Stance.Stand)
            {
                _stance = Stance.Stand; 
                motor.SetCapsuleDimensions
                (
                    radius: motor.Capsule.radius,
                    height: standHeight,
                    yOffset: standHeight * 0.5f
                );

                var pos = motor.TransientPosition;
                var rot = motor.TransientRotation;
                var mask = motor.CollidableLayers;
                if (motor.CharacterOverlap(pos, rot, _uncrouchOvelapResults, mask, QueryTriggerInteraction.Ignore) > 0)
                {
                    _requestedCrouch = true;
                    motor.SetCapsuleDimensions
                    (
                        radius: motor.Capsule.radius,
                        height: crouchHeight,
                        yOffset: crouchHeight * 0.5f
                    );
                    
                }
                else
                {
                    _stance = Stance.Stand;
                }
            }
        }

        public bool IsColliderValidForCollisions(Collider coll) => true;


        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {

        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        {

        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition,
            Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {

        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
        
        }
    
        public Transform GetCameraTarget() => cameraTarget;
    
    }
}