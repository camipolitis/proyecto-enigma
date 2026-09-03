using UnityEngine;

namespace Enigma.Player
{
    // Movimiento tercera persona: walk, run, crouch, jump 
    [RequireComponent(typeof(CharacterController))]
    public class ThirdPersonController : MonoBehaviour
    {
        [SerializeField] private PlayerInputHandler input;
        [SerializeField] private PlayerStateController state;
        [SerializeField] private Transform cameraTransform;
        // Cámara independiente: se asigna el transform del CameraRig

        [Header("Speeds")]
        [SerializeField] private float walkSpeed = 2.2f;
        [SerializeField] private float runSpeed = 4.5f;
        [SerializeField] private float crouchSpeed = 1.2f;
        // Velocidades en Inspector 

        [Header("Heights")]
        [SerializeField] private float standHeight = 1.8f;
        [SerializeField] private float crouchHeight = 1.2f;
        [SerializeField] private float heightLerpSpeed = 10f;
        // Transición del CharacterController al agacharse

        [Header("Jump / Gravity")]
        [SerializeField] private float jumpHeight = 1.2f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float rotationSpeed = 12f;

        private CharacterController _controller;
        private float _verticalVelocity;
        private bool _isCrouching;
        private float _targetHeight;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _targetHeight = standHeight;
            ApplyHeightImmediate(standHeight);
        }

        private void Update()
        {
            if (input == null || state == null)
                return;

            HandleCrouchToggle();
            ApplyHeightSmooth();

            if (!state.CanMove)
            {
                ApplyGravityOnly();
                return;
                // En intro/locked aplicamos gravedad para no flotar
            }

            Vector2 moveInput = input.MoveInput;
            Vector3 move = GetCameraRelativeMove(moveInput);

            float speed = walkSpeed;
            if (_isCrouching)
                speed = crouchSpeed;
            else if (input.SprintHeld)
                speed = runSpeed;
            // Prioridad: crouch > sprint > walk.

            if (move.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(move);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }

            if (_controller.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;
            // Mantiene contacto con el suelo 

            if (input.JumpPressed && _controller.isGrounded && !_isCrouching)
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            // Para alcanzar jumpHeight

            _verticalVelocity += gravity * Time.deltaTime;

            Vector3 velocity = move * speed + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * Time.deltaTime);
            // Un solo Move por frame 
        }

        private Vector3 GetCameraRelativeMove(Vector2 inputVec)
        {
            if (cameraTransform == null)
                return new Vector3(inputVec.x, 0f, inputVec.y).normalized;

            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
            return (forward * inputVec.y + right * inputVec.x).normalized;
            // Movimiento relativo a la cámara, no al mundo.
        }

        private void HandleCrouchToggle()
        {
            if (!input.CrouchPressedThisFrame || !state.CanMove)
                return;

            _isCrouching = !_isCrouching;
            _targetHeight = _isCrouching ? crouchHeight : standHeight;
        }

        private void ApplyHeightSmooth()
        {
            float newHeight = Mathf.MoveTowards(_controller.height, _targetHeight, heightLerpSpeed * Time.deltaTime);
            float delta = newHeight - _controller.height;
            if (Mathf.Abs(delta) < 0.0001f)
                return;

            _controller.height = newHeight;
            _controller.center = new Vector3(0f, newHeight * 0.5f, 0f);
            // Center a la mitad de la altura para que el capsule no se hunda o flote.
        }

        private void ApplyHeightImmediate(float height)
        {
            _controller.height = height;
            _controller.center = new Vector3(0f, height * 0.5f, 0f);
        }

        private void ApplyGravityOnly()
        {
            if (_controller.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;

            _verticalVelocity += gravity * Time.deltaTime;
            _controller.Move(Vector3.up * _verticalVelocity * Time.deltaTime);
        }

        public void SetProneVisual(bool prone)
        {
            _isCrouching = prone;
            _targetHeight = prone ? crouchHeight * 0.7f : standHeight;
            if (prone)
                ApplyHeightImmediate(_targetHeight);
            // Altura baja procedural hasta que tengamos la animación
        }
    }
}
