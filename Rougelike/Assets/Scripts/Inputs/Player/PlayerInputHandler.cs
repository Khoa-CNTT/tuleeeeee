using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

using tuleeeeee.Events;

namespace tuleeeeee.MyInput
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputHandler : MonoBehaviour
    {
        private Camera _camera;
        public Vector2 RawMovementInput { get; private set; }
        public int NormInputX { get; private set; }
        public int NormInputY { get; private set; }
        public bool RollInput { get; private set; }
        public bool RollInputStop { get; private set; }
        public bool ShotInput { get; private set; }
        public bool IsOpeningMenu { get; private set; }
        public bool IsOpeningMap { get; private set; }
        public bool IsSubmit { get; private set; }


        private bool isSwitchWeaponClicked = false;

        private float rollInputStartTime;
        [SerializeField]
        private float inputHoldTime = 0.2f;

        private void Awake()
        {
            _camera = Camera.main;
        }
        /// <summary>
        ///   Method called when the user input movement
        /// </summary>
        /// <param name="context"></param>
        /// 
        public void OnMove(InputAction.CallbackContext context)
        {
            RawMovementInput = context.ReadValue<Vector2>();

            NormInputX = (int)(RawMovementInput * Vector2.right).normalized.x;
            NormInputY = (int)(RawMovementInput * Vector2.up).normalized.y;
        }

        /// <summary>
        /// Method called when the user input mouse
        /// </summary>
        /// <param name="context"></param>
        public void OnAim(InputAction.CallbackContext context)
        {
            Vector2 newInput = context.ReadValue<Vector2>();

            // For mouse input (large values)
            if (Mathf.Abs(newInput.x) > 1f || Mathf.Abs(newInput.y) > 1f)
            {
                Vector3 mouseWorldPosition = _camera.ScreenToWorldPoint(
                    new Vector3(newInput.x, newInput.y, _camera.nearClipPlane));
                mouseWorldPosition.z = 0;
                LookEvent.Invoke((mouseWorldPosition - transform.position).normalized);
            }
            // For controller input (small values)
            else
            {
                if (newInput.magnitude > 0.1f) // Deadzone check
                {
                    LookEvent.Invoke(newInput.normalized);
                }
            }

            /*  if (!(newInput.normalized == newInput)) // GamePad
              {
                  Vector2 worldPos = _camera.ScreenToWorldPoint(newInput);
                  newInput = (worldPos - (Vector2)transform.position).normalized;
              }

              if (newInput.magnitude >= .9f)
              {
                  LookEvent.Invoke(newInput);
              }*/
        }

        /// <summary>
        /// Method called when the user input roll
        /// </summary>
        /// <param name="context"></param>
        public void OnRoll(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                RollInput = true;
                RollInputStop = false;
                rollInputStartTime = Time.time;
            }
            else if (context.canceled)
            {
                RollInput = false;
                RollInputStop = true;
            }
        }

        /// <summary>
        /// Fire
        /// </summary>
        /// <param name="context"></param>
        public void OnFire(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                ShotInput = true;
            }
            else if (context.canceled)
            {
                ShotInput = false;
            }
            AttackEvent.Invoke(ShotInput);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void OnScroll(InputAction.CallbackContext context)
        {
            Vector2 newInput = context.ReadValue<Vector2>();
            ScrollEvent.Invoke(newInput);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void OnSelectWeapon(InputAction.CallbackContext context)
        {
            string pressedKey = context.control.name;
            int number = int.Parse(pressedKey);

            SelectWeaponEvent.Invoke(number);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void OnSwitchWeapon(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                isSwitchWeaponClicked = !isSwitchWeaponClicked;
                SwitchWeaponEvent.Invoke(isSwitchWeaponClicked);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void OnFastSwitchWeapon(InputAction.CallbackContext context)
        {
            bool isClick = false;
            if (context.started)
            {
                isClick = true;
            }
            else if (context.canceled)
            {
                isClick = false;
            }

            FastSwitchWeaponEvent.Invoke(isClick);
        }


        /// <summary>
        ///  Reload
        /// </summary>
        /// <param name="context"></param>
        public void OnReload(InputAction.CallbackContext context)
        {
            bool isReloading = false;
            if (context.started)
            {
                isReloading = true;
            }
            else if (context.canceled)
            {
                isReloading = false;
            }
            ReloadEvent.Invoke(isReloading);
        }

        /// <summary>
        ///  Reload
        /// </summary>
        /// <param name="context"></param>
        public void OnUseItem(InputAction.CallbackContext context)
        {
            bool isUseItem = false;
            if (context.started)
            {
                isUseItem = true;
            }
            else if (context.canceled)
            {
                isUseItem = false;
            }
            UseItemEvent.Invoke(isUseItem);
        }

        /// <summary>
        ///  Reload
        /// </summary>
        /// <param name="context"></param>
        public void OnOpenMenu(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                IsOpeningMenu = !IsOpeningMenu;
                OpenMenuEvent.Invoke(IsOpeningMenu);
            }
        }

        /// <summary>
        ///  Reload
        /// </summary>
        /// <param name="context"></param>
        public void OnOpenMap(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                IsOpeningMap = !IsOpeningMap;
                OpenMapEvent.Invoke(IsOpeningMap);
            }
        }

        /// <summary>
        ///  Reload
        /// </summary>
        /// <param name="context"></param>
        public void OnSubmit(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                IsSubmit = true;
            }
            else if(context.canceled)
            {
                IsSubmit = false;
            }
        }
        public void UseRollInput() => RollInput = false;
        private void CheckRollInputHoldTime()
        {
            if (Time.time >= rollInputStartTime + inputHoldTime)
            {
                RollInput = false;
            }
        }

        #region EVENTS
        private readonly LookEvent onLookEvent = new LookEvent();
        private readonly ScrollEvent onScrollEvent = new ScrollEvent();
        private readonly SelectWeaponEvent onSelectWeaponEvent = new SelectWeaponEvent();
        private readonly SwitchWeapon onSwitchWeaponEvent = new SwitchWeapon();
        private readonly FastSwitchWeaponEvent onFastSwitchWeaponEvent = new FastSwitchWeaponEvent();
        private readonly AttackEvent onAttackEvent = new AttackEvent();
        private readonly ReloadEvent onReloadEvent = new ReloadEvent();
        private readonly UseItemEvent onUseItemEvent = new UseItemEvent();
        private readonly OpenMenuEvent onOpenMenuEvent = new OpenMenuEvent();
        private readonly OpenMapEvent onOpenMapEvent = new OpenMapEvent();
        private readonly SubmitEvent onSubmitEvent = new SubmitEvent();
        public UnityEvent<Vector2> LookEvent => onLookEvent;
        public UnityEvent<Vector2> ScrollEvent => onScrollEvent;
        public UnityEvent<int> SelectWeaponEvent => onSelectWeaponEvent;
        public UnityEvent<bool> SwitchWeaponEvent => onSwitchWeaponEvent;
        public UnityEvent<bool> FastSwitchWeaponEvent => onFastSwitchWeaponEvent;
        public UnityEvent<bool> AttackEvent => onAttackEvent;
        public UnityEvent<bool> ReloadEvent => onReloadEvent;
        public UnityEvent<bool> UseItemEvent => onUseItemEvent;
        public UnityEvent<bool> OpenMenuEvent => onOpenMenuEvent;
        public UnityEvent<bool> OpenMapEvent => onOpenMapEvent;
        public UnityEvent<bool> SubmitEvent => onSubmitEvent;
        #endregion

    }
}