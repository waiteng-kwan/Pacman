using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core
{
    /// <summary>
    /// Like the Unreal Engine version of player controller, it should take in input
    /// Should not directly control the pawn but rather pass info along to it
    /// </summary>
    public class PlayerController : Core.Controller
    {
        [field: SerializeField]
        public string PlayerHash { get; private set; } = string.Empty;

        [Header("Input")]
        [SerializeField, ReadOnly]
        private PlayerInput m_pInput;

        protected override void OnValidate()
        {
            m_pInput ??= GetComponentInChildren<PlayerInput>();
        }

        protected override void Awake()
        {
            m_pInput ??= GetComponentInChildren<PlayerInput>();

            if (GameModeBase.Instance && Index < 0)
                GameModeBase.Instance.RegisterPlayer(this);
        }

        // Update is called once per frame
        void Update()
        {
            //temp test code
            if(Input.GetKeyDown(KeyCode.W))
            {
                Pawn.SetMoveDir(Vector2.up);
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                Pawn.SetMoveDir(Vector2.left);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                Pawn.SetMoveDir(Vector2.down);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                Pawn.SetMoveDir(Vector2.right);
            }
            else if(Input.GetKeyDown(KeyCode.Escape))
            {
                Pawn.ResetMovement();
            }
        }

        public void SetPlayerHash(string hash)
        {
            PlayerHash = hash;
        }

        #region Input Messages
        private void OnDeviceLost()
        {

        }

        public void OnMove()
        {

        }
        #endregion
    }
}