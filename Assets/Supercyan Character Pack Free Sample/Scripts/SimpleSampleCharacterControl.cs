using System.Collections.Generic;
using UnityEngine;

namespace Supercyan.FreeSample
{
    public class SimpleSampleCharacterControl : MonoBehaviour
    {
        private enum ControlMode
        {
            /// <summary>
            /// Up moves the character forward, left and right turn the character gradually and down moves the character backwards
            /// </summary>
            Tank,
            /// <summary>
            /// Character freely moves in the chosen direction from the perspective of the camera
            /// </summary>
            Direct
        }
        [SerializeField] private LayerMask groundLayers;
        [SerializeField] private float groundCheckDistance = 0.1f;
        [SerializeField] private float m_moveSpeed = 2;
        [SerializeField] private float m_turnSpeed = 200;
        [SerializeField] private float m_jumpForce = 4;

        [SerializeField] private Animator m_animator = null;
        [SerializeField] private Rigidbody m_rigidBody = null;

        [SerializeField] private ControlMode m_controlMode = ControlMode.Direct;
        private bool m_jumpButtonHeld = false;
        private float m_currentV = 0;
        private float m_currentH = 0;

        private readonly float m_interpolation = 10;
        private readonly float m_walkScale = 0.33f;
        private readonly float m_backwardsWalkScale = 0.16f;
        private readonly float m_backwardRunScale = 0.66f;

        private bool m_wasGrounded;
        private Vector3 m_currentDirection = Vector3.zero;

        private float m_jumpTimeStamp = 0;
        private float m_minJumpInterval = 0.25f;
        private bool m_jumpInput = false;

        private bool m_isGrounded;

        private void Awake()
        {
            if (!m_animator) m_animator = GetComponent<Animator>();
	    if (!m_rigidBody) m_rigidBody = GetComponent<Rigidbody>();

        }


        private void Update()
        {
            // Обработка одиночного нажатия
            if (!m_jumpButtonHeld && Input.GetKeyDown(KeyCode.Space))
            {
                m_jumpInput = true;
                m_jumpButtonHeld = true;
            }

            // Сброс флага при отпускании
            if (Input.GetKeyUp(KeyCode.Space))
            {
                m_jumpButtonHeld = false;
            }
        }

       private void FixedUpdate()
        {
            Vector3 sphereCastOrigin = transform.position + Vector3.up * 0.5f;

            m_isGrounded = Physics.SphereCast(
                sphereCastOrigin,
                0.2f,
                Vector3.down,
                out RaycastHit hit,
                groundCheckDistance + 0.5f,
                groundLayers
            );

            Debug.DrawRay(sphereCastOrigin, Vector3.down * (groundCheckDistance + 0.5f), m_isGrounded ? Color.green : Color.red);

            m_animator.SetBool("Grounded", m_isGrounded);

            switch (m_controlMode)
            {
                case ControlMode.Direct:
                    DirectUpdate();
                    break;

                case ControlMode.Tank:
                    TankUpdate();
                    break;

                default:
                    Debug.LogError("Unsupported state");
                    break;
            }

            m_wasGrounded = m_isGrounded;
            m_jumpInput = false;
        }

        private void TankUpdate()
        {
            float v = Input.GetAxis("Vertical");
            float h = Input.GetAxis("Horizontal");

            bool walk = Input.GetKey(KeyCode.LeftShift);

            if (v < 0)
            {
                if (walk) { v *= m_backwardsWalkScale; }
                else { v *= m_backwardRunScale; }
            }
            else if (walk)
            {
                v *= m_walkScale;
            }

            m_currentV = Mathf.Lerp(m_currentV, v, Time.deltaTime * m_interpolation);
            m_currentH = Mathf.Lerp(m_currentH, h, Time.deltaTime * m_interpolation);

            transform.position += transform.forward * m_currentV * m_moveSpeed * Time.deltaTime;
            transform.Rotate(0, m_currentH * m_turnSpeed * Time.deltaTime, 0);

            m_animator.SetFloat("MoveSpeed", m_currentV);

            JumpingAndLanding();
        }
        private void DirectUpdate()
        {
            float v = Input.GetAxis("Vertical");
            float h = Input.GetAxis("Horizontal");

            Transform camera = Camera.main.transform;

            Vector3 forward = camera.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 right = camera.right;
            right.y = 0;
            right.Normalize();

            Vector3 direction = forward * v + right * h;

            if (direction.magnitude > 1f)
            {
                direction.Normalize();
            }

            // Применяем walkScale к итоговому вектору
            if (Input.GetKey(KeyCode.LeftShift))
            {
                direction *= m_walkScale;
            }

            m_currentDirection = Vector3.Slerp(m_currentDirection, direction, Time.deltaTime * m_interpolation);

            if (m_currentDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(m_currentDirection);

                // Движение через Rigidbody
                Vector3 newPosition = m_rigidBody.position + m_currentDirection * m_moveSpeed * Time.deltaTime;
                m_rigidBody.MovePosition(newPosition);

                m_animator.SetFloat("MoveSpeed", direction.magnitude);
            }

            JumpingAndLanding();
        }



        private void JumpingAndLanding()
        {
            bool jumpCooldownOver = (Time.time - m_jumpTimeStamp) >= m_minJumpInterval;

            if (jumpCooldownOver && m_isGrounded && m_jumpInput)
            {
                m_jumpTimeStamp = Time.time;
                m_rigidBody.AddForce(Vector3.up * m_jumpForce, ForceMode.Impulse);
            }
        }
    }
}
