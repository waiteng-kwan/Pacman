using UnityEngine;

namespace Game
{
    public class PacmanBehaviour : PawnBase
    {
        private PacmanBaseData m_data
        {
            get => Settings as PacmanBaseData;
            set => Settings = value;
        }

        //attributes
        public PacmanAttributes Attributes { get; private set; }

        #region Unity Fn
        protected override void OnValidate()
        {
            base.OnValidate();

            if (m_data == null)
                Debug.LogError("Pacman data is missing on " + gameObject.name);

            Attributes = GetComponent<PacmanAttributes>();
        }

        protected override void Awake()
        {
            base.Awake();
            Attributes = GetComponent<PacmanAttributes>();

            if (m_data != null)
            {
                ChangeModel(m_data.CharacterModel.gameObject);
            }
        }

        private void FixedUpdate()
        {
            Rigidbody.velocity = m_moveVecDir * m_data.Speed;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Ghost" && !Attributes.IsPlayerInvul())
            {
                var ghostBehaviour = other.GetComponent<GhostBehaviourBase>();
                if (ghostBehaviour == null)
                    return;

                GameModeBase.Instance.PlayerCollidedWithGhost(ghostBehaviour, this);
            }
        }
        #endregion

        protected override void InternalSetSettingsData(PawnDataBase data)
        {
            m_data = data as PacmanBaseData;

            ChangeModel(m_data.CharacterModel.gameObject);
        }

        public void SetPlayerEatGhostState(bool canEat)
        {
            Attributes.SetCanEatGhostState(canEat);
        }

        public void SetPlayerState(PacmanStates state)
        {
            Attributes.SetState(state);

            //handle state change here
            switch (state)
            {
                case PacmanStates.Alive:
                    break;
                case PacmanStates.Dead:
                    break;
                case PacmanStates.Respawning:
                    break;
                default:
                    break;
            }
        }
    }
}