using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace tuleeeeee.Events
{
    /// <summary>
    ///     An event representing a fire input 
    /// </summary>
    public class AttackEvent : UnityEvent<bool> { }

    /// <summary>
    ///     An event when player a reload input
    /// </summary>
    public class ReloadEvent : UnityEvent<bool> { }

    /// <summary>
    ///     An event when play useitem input
    /// </summary>
    public class UseItemEvent : UnityEvent<bool> { }

    /// <summary>
    ///     An event when open menu input
    /// </summary>
    public class OpenMenuEvent : UnityEvent<bool> { }

    /// <summary>
    ///     An event when open menu input
    /// </summary>
    public class OpenMapEvent : UnityEvent<bool> { }

    /// <summary>
    ///     An event when open menu input
    /// </summary>
    public class SubmitEvent : UnityEvent<bool> { }

    /// <summary>
    ///     An event representing a look input (where the character must look) in the direction of its parameter
    /// </summary>
    public class LookEvent : UnityEvent<Vector2> { }

    /// <summary>
    ///     An event representing a scroll input (y axis)
    /// </summary>
    public class ScrollEvent : UnityEvent<Vector2> { }

    /// <summary>
    ///  SelectWeapon By 1-9
    /// </summary>
    public class SelectWeaponEvent : UnityEvent<int> { }

    /// <summary>
    /// Switch Weapon
    /// </summary>
    public class SwitchWeapon : UnityEvent<bool> { }

    /// <summary>
    /// Fast Switch Weapon Q
    /// </summary>
    public class FastSwitchWeaponEvent : UnityEvent<bool> { }

}
