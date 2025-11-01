using System.Collections;
using System.Collections.Generic;
using tuleeeeee.Enums;
using tuleeeeee.Misc;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;


[RequireComponent(typeof(AimWeaponEvent))]
[DisallowMultipleComponent]
public class AimWeapon : MonoBehaviour
{
    #region Tooltip
    [Tooltip("Populate with the Transform from the child WeaponeRotationPoint gameobject")]
    #endregion
    [SerializeField] private Transform weaponRotationPointTransform;
    [SerializeField] private Transform cursor;

    private AimWeaponEvent aimWeaponEvent;
    private Player player;
    private PlayerInput playerInput;

    private void Awake()
    {
        aimWeaponEvent = GetComponent<AimWeaponEvent>();
        player = GetComponent<Player>();
        playerInput = GetComponent<PlayerInput>();
        if (playerInput != null && playerInput.currentControlScheme != Settings.gamePad)
        {
            cursor.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        aimWeaponEvent.OnWeaponAim += AimWeaponEvent_OnWeaponAim;
    }

    private void OnDisable()
    {
        aimWeaponEvent.OnWeaponAim -= AimWeaponEvent_OnWeaponAim;
    }

    private void AimWeaponEvent_OnWeaponAim(AimWeaponEvent aimWeaponEvent, AimWeaponEventArgs aimWeaponEventArgs)
    {
        Aim(aimWeaponEventArgs.aimDirection, aimWeaponEventArgs.aimAngle);
    }

    private void Aim(Direction aimDirection, float aimAngle)
    {
        //Set angle
        weaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, aimAngle);

        /* // flipping logic
         bool isFacingLeft = aimAngle > 90f || aimAngle < -90f;

         float xScale = isFacingLeft ? -1f : 1f;

         if (player != null && (player.RollState == null || !player.RollState.IsRolling))
         {
             transform.localScale = new Vector3(xScale, transform.localScale.y, transform.localScale.z);
         }
         // Flip the armPivot
         weaponRotationPointTransform.localScale = new Vector3(xScale, isFacingLeft ? -1 : 1, weaponRotationPointTransform.localScale.z);*/

        // Flip weapon
        switch (aimDirection)
        {
            case Direction.left:
            case Direction.upleft:
                transform.localScale = new Vector3(-1f, transform.localScale.y, transform.localScale.z);
                weaponRotationPointTransform.localScale = new Vector3(-1f, -1f, 0f);
                break;
            case Direction.up:
            case Direction.upright:
            case Direction.right:
            case Direction.down:
                transform.localScale = new Vector3(1f, transform.localScale.y, transform.localScale.z);
                weaponRotationPointTransform.localScale = new Vector3(1f, 1f, 0f);
                break;
        }

    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        //HelperUtilities.ValidateCheckNullValue(this, nameof(weaponRotationPointTransform), weaponRotationPointTransform);
    }

#endif
    #endregion


}
