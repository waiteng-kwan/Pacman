using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    /// <summary>
    /// Like the Unreal Engine version of player controller, it should take in input
    /// Should not directly control the pawn but rather pass info along to it
    /// </summary>
    public class PlayerController : Game.Controller
    {
        [field: SerializeField]
        public string PlayerHash { get; private set; } = string.Empty;

        [Header("Characters")]
        [SerializeField]
        private PacmanBehaviour m_playerCharacter;
        public PacmanBehaviour PlayerCharacter => m_playerCharacter;

        [Header("Input")]
        [SerializeField, ReadOnly]
        private PlayerInput m_pInput;

        protected override void OnValidate()
        {
            m_pInput = GetComponentInChildren<PlayerInput>();
        }

        protected override void Awake()
        {
            if(!m_pInput)
                m_pInput = GetComponentInChildren<PlayerInput>();

            if (GameModeBase.Instance && Index < 0)
                GameModeBase.Instance.RegisterPlayer(this);
        }

        // Update is called once per frame
        void Update()
        {
            //temp test code
            if(Input.GetKeyDown(KeyCode.W))
            {
                m_playerCharacter.SetMoveDir(Vector2.up);
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                m_playerCharacter.SetMoveDir(Vector2.left);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                m_playerCharacter.SetMoveDir(Vector2.down);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                m_playerCharacter.SetMoveDir(Vector2.right);
            }
            else if(Input.GetKeyDown(KeyCode.Escape))
            {
                m_playerCharacter.ResetMovement();
            }
        }

        public void PossessCharacter(PacmanBehaviour character)
        {
            character.SetOwner(this);

            m_playerCharacter = character;
        }

        public void SetPlayerHash(string hash)
        {
            PlayerHash = hash;
        }
    }
}