﻿namespace ShiftingDungeon.Character.Player
{
    using UnityEngine;
    using Util;
    using Weapons;

    public class PlayerBehavior : MonoBehaviour
    {
        [SerializeField]
        private int maxHealth = 10;
        [SerializeField]
        private float acceleration = 1;
        [SerializeField]
        private float maxSpeed = 5;
        [SerializeField]
        private Weapon[] weapons = null;

        private Animator anim = null;
        private Rigidbody2D rgby = null;
        private PlayerInput input = null;
        private StateMap stateMap = null;
        private bool doOnce = false;
        private int attackFinishedHash = 0;
        private int hitHash = 0;

        /// <summary> The player's current health. </summary>
        public int Health { get; private set; }
        /// <summary> The player's current weapon. </summary>
        public int CurrentWeapon { get; private set; }
        /// <summary> The player's current state. </summary>
        public Enums.PlayerState CurrentState { get; private set; }

        private void Start()
        {
            this.anim = GetComponent<Animator>();
            this.rgby = GetComponent<Rigidbody2D>();
            this.input = GetComponent<PlayerInput>();
            this.stateMap = new StateMap();
            this.doOnce = false;
            this.attackFinishedHash = Animator.StringToHash("AttackFinished");
            this.hitHash = Animator.StringToHash("Hit");
            this.Health = this.maxHealth;
            this.CurrentWeapon = 0;
            this.CurrentState = Enums.PlayerState.Idle;

            foreach (Weapon w in this.weapons)
            {
                w.Init();
                w.CleanUp();
            }
        }

        private void Update()
        {
            Enums.PlayerState temp = this.CurrentState;
            this.CurrentState = this.stateMap.GetState(this.anim.GetCurrentAnimatorStateInfo(0).fullPathHash);
            if (temp != CurrentState)
            {
                this.doOnce = false;
                this.anim.SetBool(this.attackFinishedHash, false);                
            }

            switch(this.CurrentState)
            {
                case Enums.PlayerState.Idle: Idle(); break;
                case Enums.PlayerState.Move: Move(); break;
                case Enums.PlayerState.Attack: Attack(); break;
                case Enums.PlayerState.Hurt: Hurt(); break;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.collider.gameObject.tag == "Enemy")
                anim.SetTrigger(this.hitHash);
        }

        /// <summary> Cycles to the next weapon in the players list. </summary>
        public void GoToNextWeapon()
        {
            this.CurrentWeapon++;
            if (this.CurrentWeapon >= this.weapons.Length)
                this.CurrentWeapon = 0;
        }

        /// <summary> Cycles to the previous weapon in the players list. </summary>
        public void GoToPreviousWeapon()
        {
            this.CurrentWeapon--;
            if (this.CurrentWeapon < 0)
                this.CurrentWeapon = this.weapons.Length - 1;
        }

        private void Idle()
        {
        }

        private void Move()
        {
            int x;
            int y;
            if (this.input.Up)
                y = 1;
            else if (this.input.Down)
                y = -1;
            else
                y = 0;

            if (this.input.Left)
                x = -1;
            else if (this.input.Right)
                x = 1;
            else
                x = 0;

            Vector2 dir = new Vector2(x, y);
            if(dir != Vector2.zero)
                this.transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, dir));
            Vector2 right = this.transform.right;
            Vector2 speed = this.rgby.velocity + right * this.acceleration;
            if (speed.magnitude < this.maxSpeed)
                this.rgby.velocity = speed;
        }

        private void Attack()
        {
            if(!this.doOnce)
            {
                this.weapons[this.CurrentWeapon].ReInit();
                this.doOnce = true;
            }
            
            if(this.weapons[this.CurrentWeapon].WeaponUpdate())
            {
                anim.SetBool(this.attackFinishedHash, true);
                this.weapons[this.CurrentWeapon].CleanUp();
            }
        }

        private void Hurt()
        {
            if(!this.doOnce)
            {
                this.Health--;
                this.doOnce = true;
            }
        }
    }
}
