using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace Complete
{
    public class TankShooting : MonoBehaviourPunCallbacks
    {
        public int m_PlayerNumber = 1;              // Used to identify the different players.
        public Rigidbody m_Shell;                   // Prefab of the shell.
        public Transform m_FireTransform;           // A child of the tank where the shells are spawned.
        public Slider m_AimSlider;                  // A child of the tank that displays the current launch force.
        public AudioSource m_ShootingAudio;         // Reference to the audio source used to play the shooting audio. NB: different to the movement audio source.
        public AudioClip m_ChargingClip;            // Audio that plays when each shot is charging up.
        public AudioClip m_FireClip;                // Audio that plays when each shot is fired.
        public float m_MinLaunchForce = 15f;        // The force given to the shell if the fire button is not held.
        public float m_MaxLaunchForce = 30f;        // The force given to the shell if the fire button is held for the max charge time.
        public float m_MaxChargeTime = 0.75f;       // How long the shell can charge for before it is fired at max force.


        private string m_FireButton;                // The input axis that is used for launching shells.
        private float m_CurrentLaunchForce;         // The force that will be given to the shell when the fire button is released.
        private float m_ChargeSpeed;                // How fast the launch force increases, based on the max charge time.
        private bool m_Fired;                       // Whether or not the shell has been launched with this button press.

        private string m_TurretAxisName;            // The name of the input axis for turning turret
        private GameObject m_TurrentAnchors;        // A anchor of aimslider for precenting aim correctly
        public float m_TurnTurretSpeed = 120f;      // How fast the tank turns in degrees per second
        public float m_TurretAngleLimit = 60f;      // The maximum turning angle of turret.
        private float m_TurretAngle = 0f;           // The current turning angle of turret.
        private GameObject m_Turret;                // Reference used to turn turrent.
        private float m_TurnTurretValue;            // The current value of the turn input.

        private void OnEnable()
        {
            base.OnEnable();
            // When the tank is turned on, reset the launch force and the UI
            m_CurrentLaunchForce = m_MinLaunchForce;
            m_AimSlider.value = m_MinLaunchForce;

            m_Turret = transform.FindAnyChild<Transform>("TankTurret").gameObject;
            m_TurnTurretValue = 0f;
            m_TurrentAnchors = transform.FindAnyChild<Transform>("TurretAnchor").gameObject;
        }


        private void Start()
        {
            // The fire axis is based on the player number.
            m_FireButton = "Fire" + m_PlayerNumber;

            // The rate that the launch force charges up is the range of possible forces by the max charge time.
            m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;

            m_TurretAxisName = "HorizontalTurret";
        }


        private void Update()
        {
            // The slider should have a default value of the minimum launch force.
            m_AimSlider.value = m_MinLaunchForce;

            // If the max force has been exceeded and the shell hasn't yet been launched...
            if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
            {
                // ... use the max force and launch the shell.
                m_CurrentLaunchForce = m_MaxLaunchForce;
                Fire();
            }
            // Otherwise, if the fire button has just started being pressed...
            else if (Input.GetButtonDown(m_FireButton))
            {
                // ... reset the fired flag and reset the launch force.
                m_Fired = false;
                m_CurrentLaunchForce = m_MinLaunchForce;

                // Change the clip to the charging clip and start it playing.
                m_ShootingAudio.clip = m_ChargingClip;
                m_ShootingAudio.Play();
            }
            // Otherwise, if the fire button is being held and the shell hasn't been launched yet...
            else if (Input.GetButton(m_FireButton) && !m_Fired)
            {
                // Increment the launch force and update the slider.
                m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;

                m_AimSlider.value = m_CurrentLaunchForce;
            }
            // Otherwise, if the fire button is released and the shell hasn't been launched yet...
            else if (Input.GetButtonUp(m_FireButton) && !m_Fired)
            {
                // ... launch the shell.
                Fire();
            }

            // Adjust the turret angle
            m_TurnTurretValue = Input.GetAxis(m_TurretAxisName);
        }

        void FixedUpdate()
        {
            TurnTurrent();
        }

        private void TurnTurrent()
        {
            // Determine the number of degrees to be turned based on the input, speed and time between frames.
            float turn = m_TurnTurretValue * m_TurnTurretSpeed * Time.deltaTime;
            m_TurretAngle += turn;

            if (Mathf.Abs(m_TurretAngle) > m_TurretAngleLimit)
            {
                // Fix maximum turret turning angle
                m_TurretAngle = (m_TurretAngle>0)? m_TurretAngleLimit:-m_TurretAngleLimit;
            }
            else
            {
                // Apply this rotation to the turret rotation and aimslider
                m_Turret.GetComponent<Transform>().Rotate(Vector3.up, turn);
                m_TurrentAnchors.GetComponent<RectTransform>().Rotate(Vector3.back, turn);
            }

        }

        private void Fire()
        {
            // Set the fired flag so only Fire is only called once.
            m_Fired = true;

            // Create an instance of the shell and store a reference to it's rigidbody.
            Rigidbody shellInstance =
                Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;

            photonView.RPC("FireOther", RpcTarget.Others, m_FireTransform.position, m_CurrentLaunchForce);

            // Set the shell's velocity to the launch force in the fire position's forward direction.
            shellInstance.velocity = m_CurrentLaunchForce * m_FireTransform.forward;

            // Change the clip to the firing clip and play it.
            m_ShootingAudio.clip = m_FireClip;
            m_ShootingAudio.Play();

            // Reset the launch force.  This is a precaution in case of missing button events.
            m_CurrentLaunchForce = m_MinLaunchForce;
        }

        [PunRPC]
        private void FireOther(Vector3 pos, float force)
        {
            m_Fired = true;

            Rigidbody shellInstance = Instantiate(m_Shell, pos, m_FireTransform.rotation) as Rigidbody;
            shellInstance.velocity = force * m_FireTransform.forward;
            m_CurrentLaunchForce = m_MinLaunchForce;
        }
    }
}