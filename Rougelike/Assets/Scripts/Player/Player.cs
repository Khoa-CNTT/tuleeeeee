using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using tuleeeeee.Data;
using tuleeeeee.Managers;
using tuleeeeee.Misc;
using tuleeeeee.MyInput;
using tuleeeeee.StateMachine;
using tuleeeeee.Utilities;
using UnityEngine;

/*[RequireComponent(typeof(Health))]
[RequireComponent(typeof(InputHandler))]
[RequireComponent(typeof(SortingGroup))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[DisallowMultipleComponent]*/
public class Player : MonoBehaviour
{
    #region CORECOMPOMENTS
    public Health Health { get => health != null ? health : Core.GetCoreComponent(ref health); }
    private Health health;
    #endregion

    #region STATES
    public StateManager StateManager { get; private set; }
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerRollState RollState { get; private set; }
    public PlayerDeadState DeadState { get; private set; }
    #endregion
    public PlayerDetailsSO PlayerDetails { get; private set; }

    public MovementDetailsSO MovementDetails;

    #region UNITYCOMPOMENTS
    public Core Core { get; private set; }
    public PlayerInputHandler InputHandler { get; private set; }
    public SpriteRenderer[] SpriteRendererArray { get; private set; }
    public CircleCollider2D CircleCollider2D { get; private set; }
    public Animator Animator { get; private set; }
    #endregion

    public float MoveSpeed { get; private set; }

    #region EVENTS
    public AimWeaponEvent AimWeaponEvent { get; private set; }
    public SetActiveWeaponEvent SetActiveWeaponEvent { get; private set; }
    public ActiveWeapon ActiveWeapon { get; private set; }
    public FireWeaponEvent FireWeaponEvent { get; private set; }
    public WeaponFiredEvent WeaponFiredEvent { get; private set; }
    public ReloadWeaponEvent ReloadWeaponEvent { get; private set; }
    public WeaponReloadedEvent WeaponReloadedEvent { get; private set; }
    public StopReloadWeaponEvent StopReloadWeaponEvent { get; private set; }
    public HealthEvent HealthEvent { get; private set; }
    public DestroyedEvent DestroyedEvent { get; private set; }
    #endregion

    public List<Weapon> weaponList = new List<Weapon>();

    private bool isPlayerMovementDisabled = false;
    private void OnEnable()
    {
        HealthEvent.OnHealthChanged += HealthEvent_OnHealthChanged;
        InputHandler.UseItemEvent.AddListener(UsedItem);
        InputHandler.OpenMenuEvent.AddListener(OpenPauseMenu);
        InputHandler.OpenMapEvent.AddListener(OpenMap);
    }
    private void OnDisable()
    {
        HealthEvent.OnHealthChanged -= HealthEvent_OnHealthChanged;
        InputHandler.UseItemEvent.RemoveListener(UsedItem);
        InputHandler.OpenMenuEvent.RemoveListener(OpenPauseMenu);
        InputHandler.OpenMapEvent.RemoveListener(OpenMap);
    }
    private void Awake()
    {
        #region Compoments
        Core = GetComponentInChildren<Core>();
        InputHandler = GetComponent<PlayerInputHandler>();
        SpriteRendererArray = GetComponentsInChildren<SpriteRenderer>();
        CircleCollider2D = GetComponentInChildren<CircleCollider2D>();
        Animator = GetComponent<Animator>();

        StateManager = new StateManager();
        #endregion
        #region Events
        AimWeaponEvent = GetComponent<AimWeaponEvent>();
        SetActiveWeaponEvent = GetComponent<SetActiveWeaponEvent>();
        ActiveWeapon = GetComponent<ActiveWeapon>();
        FireWeaponEvent = GetComponent<FireWeaponEvent>();
        WeaponFiredEvent = GetComponent<WeaponFiredEvent>();
        ReloadWeaponEvent = GetComponent<ReloadWeaponEvent>();
        WeaponReloadedEvent = GetComponent<WeaponReloadedEvent>();
        StopReloadWeaponEvent = GetComponent<StopReloadWeaponEvent>();
        HealthEvent = GetComponentInChildren<HealthEvent>();
        DestroyedEvent = GetComponent<DestroyedEvent>();
        #endregion
    }
    private void Update()
    {
        if (isPlayerMovementDisabled) return;
        Core.LogicUpdate();
        StateManager.CurrentPlayerState.LogicUpdate();
    }
    public void OpenPauseMenu(bool isOpening)
    {
        GameManager.Instance.PauseGameMenu();
    }
    public void OpenMap(bool isOpening)
    {
        GameManager.Instance.DungeonOverviewMap();
    }
    public void UsedItem(bool isUsedItem)
    {
        UseItemInput(isUsedItem);
    }
    private void UseItemInput(bool isUsedItem)
    {
        if (isUsedItem)
        {
            float useItemRadius = 2.5f;

            Collider2D[] collider2DArray = Physics2D.OverlapCircleAll(transform.position, useItemRadius);

            foreach (Collider2D collider2D in collider2DArray)
            {
                IUseable iUseable = collider2D.GetComponent<IUseable>();

                if (iUseable != null)
                {
                    iUseable.UseItem();
                }
            }
        }
    }
    private void FixedUpdate()
    {
        StateManager.CurrentPlayerState.PhysicUpdate();
    }
    public void Initialize(PlayerDetailsSO playerDetailsSO)
    {
        this.PlayerDetails = playerDetailsSO;

        MoveSpeed = MovementDetails.GetMoveSpeed();

        SetPlayerHealth();

        CreatePlayerStartingWeapon();

        SetPlayerAnimationSpeed();

        IdleState = new PlayerIdleState(this, StateManager, MovementDetails, Settings.isIdle);
        MoveState = new PlayerMoveState(this, StateManager, MovementDetails, Settings.isMoving);
        RollState = new PlayerRollState(this, StateManager, MovementDetails, Settings.isRolling);
        DeadState = new PlayerDeadState(this, StateManager, MovementDetails, Settings.isDead);

        StateManager.Initialize(IdleState);
    }

    private void HealthEvent_OnHealthChanged(HealthEvent healthEvent, HealthEventArgs healthEventArgs)
    {
        HelperUtilities.ShakeCinemachineCamera(2f, 1f);
        if (healthEventArgs.healthAmount <= 0f)
        {
            StateManager.ChangeState(DeadState);
        }
    }

    private void SetPlayerHealth()
    {
        Health.SetStartingHealth(PlayerDetails.playerHealthAmount);
    }

    private void SetPlayerAnimationSpeed()
    {
        Animator.speed = MoveSpeed / Settings.baseSpeedForPlayerAnimation;
    }

    private void CreatePlayerStartingWeapon()
    {
        weaponList.Clear();

        foreach (WeaponDetailsSO weaponDetails in PlayerDetails.startingWeaponList)
        {
            AddWeaponToPlayer(weaponDetails);
        }
    }

    public Weapon AddWeaponToPlayer(WeaponDetailsSO weaponDetails)
    {
        Weapon weapon = new Weapon()
        {
            weaponDetails = weaponDetails,
            weaponReloadTimer = 0f,
            weaponClipRemainingAmmo = weaponDetails.weaponClipAmmoCapacity,
            weaponRemainingAmmo = weaponDetails.weaponAmmoCapacity,
            isWeaponReloading = false
        };

        weaponList.Add(weapon);

        weapon.weaponListPosition = weaponList.Count;

        SetActiveWeaponEvent.CallSetActiveWeaponEvent(weapon);

        return weapon;
    }

    public Vector3 GetPlayerPosition()
    {
        return transform.position;
    }

    public void EnablePlayer()
    {
        isPlayerMovementDisabled = false;
    }

    public void DisablePlayer()
    {
        isPlayerMovementDisabled = true;
        StateManager.ChangeState(IdleState);
    }

    public void TeleportToPosition(Vector3 position)
    {
        DisablePlayer();
        // Set the new position
        transform.position = position;

        StartCoroutine(EnableAfterDelay(1f));
    }
    private IEnumerator EnableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        EnablePlayer();
    }
    public bool IsWeaponHeldByPlayer(WeaponDetailsSO weaponDetails)
    {
        foreach (Weapon weapon in weaponList)
        {
            if (weapon.weaponDetails == weaponDetails) return true;
        }
        return false;
    }
    public void AnimationTrigger() => StateManager.CurrentPlayerState.AnimationTrigger();

    public void AnimationFinishedTrigger() => StateManager.CurrentPlayerState.AnimationFinishedTrigger();

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(MovementDetails), MovementDetails);
    }

#endif
    #endregion
}
