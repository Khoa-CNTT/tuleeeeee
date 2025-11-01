using System.Collections;
using System.Collections.Generic;
using tuleeeeee.Enums;
using tuleeeeee.Misc;
using tuleeeeee.MyInput;
using tuleeeeee.Utilities;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    private Vector2 aimDirection;
    private Vector2 scrollValue;
    private int weaponSlotIndex;
    private bool fastSwitchWeapon;
    private bool isShooting;

    private PlayerInputHandler inputHandler;
    private Player player;

    private Vector3 currentWeaponDirection;
    private float currentWeaponAngle;
    private float currentPlayerAngle;
    private Direction currentAimDirection;

    private bool leftMouseDownPreviousFrame = false;

    private float fireTime = 0;

    private bool reverse = false;

    private int currentWeaponIndex = 1;
    private int previousWeaponIndex = 0;

    private void Awake()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
        player = GetComponent<Player>();
    }
    private void Start()
    {
        SetStartingWeapon();
    }
    private void OnEnable()
    {
        inputHandler.LookEvent.AddListener(AimWeapon);
        inputHandler.AttackEvent.AddListener(WeaponShooted);
        inputHandler.ScrollEvent.AddListener(ScrollSelectedWeapon);
        inputHandler.SwitchWeaponEvent.AddListener(SwitchWeapon);
        inputHandler.SelectWeaponEvent.AddListener(SelectWeapon);
        inputHandler.FastSwitchWeaponEvent.AddListener(FastSwitchWeapon);
        inputHandler.ReloadEvent.AddListener(ReloadWeapon);
    }
    private void OnDisable()
    {
        inputHandler.LookEvent.RemoveListener(AimWeapon);
        inputHandler.AttackEvent.RemoveListener(WeaponShooted);
        inputHandler.ScrollEvent.RemoveListener(ScrollSelectedWeapon);
        inputHandler.SwitchWeaponEvent.RemoveListener(SwitchWeapon);
        inputHandler.SelectWeaponEvent.RemoveListener(SelectWeapon);
        inputHandler.FastSwitchWeaponEvent.RemoveListener(FastSwitchWeapon);
        inputHandler.ReloadEvent.RemoveListener(ReloadWeapon);
    }
    private void Update()
    {
        if (isShooting)
        {
            fireTime += Time.deltaTime;
            FireWeaponInput(isShooting, currentWeaponDirection, currentWeaponAngle, currentPlayerAngle, currentAimDirection);
        }
    }
    public void WeaponShooted(bool shootInput)
    {
        isShooting = shootInput;
        // FireWeaponInput(isShooting, currentWeaponDirection, currentWeaponAngle, currentPlayerAngle, currentAimDirection);
    }
    public void ReloadWeapon(bool reloadInput)
    {
        Weapon currentWeapon = player.ActiveWeapon.GetCurrentWeapon();
        int weaponClipAmmoCapacity = currentWeapon.weaponDetails.weaponClipAmmoCapacity;
        int weaponClipRemainingAmmo = currentWeapon.weaponClipRemainingAmmo;
        bool hasInfiniteAmmo = currentWeapon.weaponDetails.hasInfiniteAmmo;
        bool hasInfiniteClipCapacity = currentWeapon.weaponDetails.hasInfiniteClipCapacity;

        if (currentWeapon.isWeaponReloading) return;

        if (currentWeapon.weaponRemainingAmmo <= 0 && !hasInfiniteAmmo) return;

        // Clip is full
        if (weaponClipRemainingAmmo == weaponClipAmmoCapacity) return;

        if (reloadInput || (!hasInfiniteClipCapacity && currentWeapon.weaponClipRemainingAmmo <= 0))
        {
            player.ReloadWeaponEvent.CallReloadWeaponEvent(currentWeapon, 0);
        }
    }
    public void AimWeapon(Vector2 newAimDirection)
    {
        aimDirection = newAimDirection;
        CalculateAimParameters();
        AimWeapon(currentWeaponDirection, currentWeaponAngle, currentPlayerAngle, currentAimDirection);
    }
    public void ScrollSelectedWeapon(Vector2 newScrollValue)
    {
        scrollValue = newScrollValue;
        SwitchWeaponInput(scrollValue);
    }
    public void SelectWeapon(int newWeaponSlotIndex)
    {
        weaponSlotIndex = newWeaponSlotIndex;
        SetWeaponByIndex(weaponSlotIndex);
    }
    public void SwitchWeapon(bool isClicked)
    {
        SwitchBetweenTwoWeapon();
    }
    public void FastSwitchWeapon(bool isClicked)
    {
        FastSwitchWeapon();
    }
    private void CalculateAimParameters()
    {
        currentWeaponDirection = aimDirection;
        currentWeaponAngle = HelperUtilities.GetAngleFromVector(currentWeaponDirection);
        currentPlayerAngle = HelperUtilities.GetAngleFromVector(aimDirection);
        currentAimDirection = HelperUtilities.GetDirection(currentPlayerAngle);
    }
    private void AimWeapon(Vector3 weaponDirection, float weaponAngle, float playerAngle, Direction playerAimDirection)
    {
        player.AimWeaponEvent.CallAimWeaponEvent(playerAimDirection, playerAngle, weaponAngle, weaponDirection);
    }

    private void FireWeaponInput(bool isShoot, Vector3 weaponDirection, float weaponAngle, float playerAngle, Direction playerAimDirection)
    {
        if (isShoot && !player.RollState.IsRolling)
        {
            player.FireWeaponEvent.CallFireWeaponEvent(true, leftMouseDownPreviousFrame, playerAimDirection, playerAngle, weaponAngle, weaponDirection, fireTime);
            leftMouseDownPreviousFrame = true;
        }
        else
        {
            fireTime = 0;
            leftMouseDownPreviousFrame = false;
        }
    }

    private void SwitchWeaponInput(Vector2 newScrollValue)
    {
        if (newScrollValue.y < 0f)
            PreviousWeapon();
        else if (newScrollValue.y > 0f)
            NextWeapon();
    }


    private void SetStartingWeapon()
    {
        int index = 1;
        foreach (Weapon weapon in player.weaponList)
        {
            if (weapon.weaponDetails == player.PlayerDetails.startingWeapon)
            {
                SetWeaponByIndex(index);
                break;
            }
            index++;
        }
    }
    private void SetWeaponByIndex(int weaponIndex)
    {
        Weapon currentWeapon = player.ActiveWeapon.GetCurrentWeapon();

        if (weaponIndex - 1 < player.weaponList.Count)
        {
            previousWeaponIndex = currentWeaponIndex;
            currentWeaponIndex = weaponIndex;
            Weapon weapon = player.weaponList[weaponIndex - 1];
            if (weapon == currentWeapon) return;
            player.StopReloadWeaponEvent.CallStopReloadWeapon(currentWeapon);
            player.SetActiveWeaponEvent.CallSetActiveWeaponEvent(weapon);
        }
    }

    private void PreviousWeapon()
    {
        if (currentWeaponIndex > 1)
        {
            currentWeaponIndex--;
        }
        else
        {
            reverse = false; // Reached the start, change direction
            currentWeaponIndex++; // Immediately reverse
        }
    }

    private void NextWeapon()
    {
        if (currentWeaponIndex < player.weaponList.Count)
        {
            currentWeaponIndex++;
        }
        else
        {
            reverse = true; // Reached the end, reverse direction
            currentWeaponIndex--; // Immediately reverse
        }
    }

    private void SwitchBetweenTwoWeapon()
    {
        if (player.weaponList.Count <= 1) return; // No switching needed
        if (!reverse)
        {
            NextWeapon();

        }
        else
        {
            PreviousWeapon();
        }
        SetWeaponByIndex(currentWeaponIndex);
    }

    private void FastSwitchWeapon()
    {
        if (previousWeaponIndex != currentWeaponIndex && previousWeaponIndex > 0)
        {
            SetWeaponByIndex(previousWeaponIndex);
        }
    }
    private void SetCurrentWeaponToFirstInTheList()
    {
        List<Weapon> tempWeaponList = new List<Weapon>();

        Weapon currentWeapon = player.weaponList[currentWeaponIndex - 1];
        currentWeapon.weaponListPosition = 1;
        tempWeaponList.Add(currentWeapon);

        int index = 2;
        foreach (Weapon weapon in player.weaponList)
        {
            if (weapon == currentWeapon) continue;
            tempWeaponList.Add(weapon);
            weapon.weaponListPosition = index;
            index++;
        }

        player.weaponList = tempWeaponList;

        currentWeaponIndex = 1;

        SetWeaponByIndex(currentWeaponIndex);
    }

    #region ANIMATION
    private void AimAnimation(Vector2 direction)
    {
        float angle = HelperUtilities.GetAngleFromVector(direction);
        Direction aimDirection = HelperUtilities.GetDirection(angle);

        InitializeAimAnimationParameters();
        SetAimWeaponAnimationParamters(aimDirection);
    }
    private void InitializeAimAnimationParameters()
    {
        player.Animator.SetBool(Settings.aimUp, false);
        player.Animator.SetBool(Settings.aimUpRight, false);
        player.Animator.SetBool(Settings.aimRight, false);
        player.Animator.SetBool(Settings.aimDown, false);
    }
    private void SetAimWeaponAnimationParamters(Direction aimDirection)
    {
        switch (aimDirection)
        {
            case Direction.up:
                player.Animator.SetBool(Settings.aimUp, true);
                break;
            case Direction.upleft:
            case Direction.upright:
                player.Animator.SetBool(Settings.aimUpRight, true);
                break;
            case Direction.left:
            case Direction.right:
                player.Animator.SetBool(Settings.aimRight, true);
                break;
            case Direction.down:
                player.Animator.SetBool(Settings.aimDown, true);
                break;
        }
    }
    #endregion

}
