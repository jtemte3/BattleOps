using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/ControlSchemeManager")]
public class ControlSchemeManager : ScriptableObject
{
    [Header("Movement")]
    public KeyCode foreward = KeyCode.W;
    public KeyCode left = KeyCode.A;
    public KeyCode right = KeyCode.D;
    public KeyCode backward = KeyCode.S;
    public KeyCode sprint = KeyCode.LeftShift;
    public KeyCode jump = KeyCode.Space;
    public KeyCode croutch = KeyCode.LeftControl;
    public KeyCode leanLeft = KeyCode.Q;
    public KeyCode leanRight = KeyCode.E;
    public bool verticalInversion = false;

    [Header("General Controls")]
    public KeyCode escapeMenu = KeyCode.Escape;
    public KeyCode interact = KeyCode.F;

    [Header("Weapon Controls")]
    public KeyCode weaponFire = KeyCode.Mouse0;
    public KeyCode weaponAimDownSights = KeyCode.Mouse1;
    public KeyCode weaponMode = KeyCode.Mouse2;
    public KeyCode weaponReload = KeyCode.R;

    [Header("Debug Controls")]
    public KeyCode debugMenu = KeyCode.BackQuote;
    public KeyCode flyingMode = KeyCode.X;
    public KeyCode flyingUp = KeyCode.Space;
    public KeyCode flyingDown = KeyCode.C;
    public KeyCode toggleHUD = KeyCode.F1;
}
