using System;
using System.Collections;
using UnityEngine;
using Util.SystemUtils;

public class FirstPersonPlayerCharacterInitData : IPlayerCharacterInitData
{
    public CharacterSettings CharacterSettings { get; set; }
    public Player Player { get; set; }
    public PausePlayerCharacter PausePlayerCharacter { get; set; }

    public Camera Camera { get; set; }
}

public class FirstPersonPlayerCharacterUpdateData : IPlayerCharacterUpdateData
{
    #region Input
    public float DeltaTime { get; set; }

    public Vector2 MovementInput { get; set; }
    public bool JumpPressedThisFrame { get; set; }
    public bool SprintPressedThisFrame { get; set; }

    long m_InputCounter;
    public long NextInputOrder() => ++m_InputCounter;

    public bool ShootHeld { get; set; }
    public bool ShootReleased { get; set; }
    public long ShootStartedOrder { get; set; }

    public long MeleeOrder { get; set; }

    public long ReloadOrder { get; set; }

    public long EquipOrder { get; set; }
    public Type EquipTarget { get; private set; }

    public void RequestEquip<T>() where T : Gun
    {
        EquipOrder = NextInputOrder();
        EquipTarget = typeof(T);
    }

    public void ClearWeaponRequests()
    {
        ShootReleased = false;
        ShootStartedOrder = 0;
        MeleeOrder = 0;
        ReloadOrder = 0;
        EquipOrder = 0;
        EquipTarget = null;
    }
    #endregion Input

    #region Output
    public bool IsSprinting { get; set; }

    public bool DoRecoil { get; set; }
    public float RecoilAmount { get; set; }
    #endregion
}

[
    RequireComponent(
        typeof(CharacterController),
        typeof(WeaponHandlerComponent),
        typeof(HealthComponent)
    ),
    RequireComponent(
        typeof(PlayerLevelComponent)
    )
]
public class FirstPersonPlayerCharacter : PlayerCharacter
{
    static readonly float s_Epsilon = 0.05f;
    static readonly float s_SqrEpsilon = s_Epsilon * s_Epsilon;

    static string AttackAnimation(Weapon weapon) => weapon switch
    {
        Pistol => "PistolShoot",
        Rifle => "RifleShoot",
        GrenadeLauncher => "GrenadeLauncherShoot",
        Shotgun => "ShotgunShoot",
        Bat => "BatBash",
        Katana => "KatanaSlice",
        _ => string.Empty
    };

    static string ReloadAnimation(Gun gun) => gun switch
    {
        Pistol => "PistolReload",
        Rifle => "RifleReload",
        GrenadeLauncher => "GrenadeLauncherReload",
        Shotgun => "ShotgunReload",
        _ => string.Empty
    };

    static IEnumerator ReloadCheckEnd(FirstPersonPlayerCharacter self, Gun gun) => gun switch
    {
        Pistol => self.CheckForPistolReloadEnd(),
        Rifle => self.CheckForRifleReloadEnd(),
        GrenadeLauncher => self.CheckForGrenadeLauncherReloadEnd(),
        Shotgun => self.CheckForShotgunReloadEnd(),
        _ => null
    };

    [SerializeField]
    GameObject m_Arms;
    [SerializeField]
    Animator m_ArmsAnimator;

    Camera m_Camera;
    CharacterSettings m_CharacterSettings;
    CharacterController m_CC;
    WeaponHandlerComponent m_WeaponComponent;
    HealthComponent m_HealthComponent;

    bool m_bIsGrounded;
    bool m_bBumpedHead;

    Vector3 m_Velocity;
    bool m_bIsJumping;
    bool m_bIsFalling;

    float m_CurrentJumpBufferTimer;
    float m_CurrentCoyoteTimer;

    bool m_bIsSprinting;
    float m_MovementSpeed;

    bool m_bMeleeing = false;

    bool m_bReloading = false;
    bool m_bReloadCancelled = false;

    public override bool HasBeenInitialised { get; protected set; }

    public override string ActionMap => "Player";
    public override bool MouseVisible => false;
    public override bool DoCameraRotation => true;

    PausePlayerCharacter m_PausePlayerCharacter;

    void Awake()
    {
        m_CC = GetComponent<CharacterController>();
        m_WeaponComponent = GetComponent<WeaponHandlerComponent>();
        m_HealthComponent = GetComponent<HealthComponent>();
    }

    public override void Init(IPlayerCharacterInitData playerCharacterInitData)
    {
        FirstPersonPlayerCharacterInitData initData = Sys.AssertType<FirstPersonPlayerCharacterInitData>(playerCharacterInitData, nameof(playerCharacterInitData));
        m_CharacterSettings = initData.CharacterSettings;
        m_PausePlayerCharacter = initData.PausePlayerCharacter;
        m_Camera = initData.Camera;
        m_WeaponComponent.Init();
        m_HealthComponent.InitHealth();
        HasBeenInitialised = true;
    }

    public override void UpdateCharacter(ref IPlayerCharacterUpdateData playerCharacterUpdateData)
    {
        Sys.Assert(HasBeenInitialised, "FirstPersonPlayerCharacter hasn't been initialised");
        FirstPersonPlayerCharacterUpdateData updateData = Sys.AssertType<FirstPersonPlayerCharacterUpdateData>(playerCharacterUpdateData, nameof(playerCharacterUpdateData));
        HandleMovement(ref updateData);
        m_WeaponComponent.UpdateHandler(updateData.DeltaTime);
        HandleWeapons(ref updateData);
    }

    public override void OnPausePressed()
    {
        Sys.Assert(HasBeenInitialised, "FirstPersonPlayerCharacter hasn't been initialised");
        m_PausePlayerCharacter.PauseGame(this);
    }

    public override void OnCharacterEnter()
    {
        Sys.Assert(HasBeenInitialised, "FirstPersonPlayerCharacter hasn't been initialised");
        if (m_Arms) m_Arms.SetActive(true);
    }

    public override void OnCharacterExit()
    {
        Sys.Assert(HasBeenInitialised, "FirstPersonPlayerCharacter hasn't been initialised");
        if (m_Arms) m_Arms.SetActive(false);
    }

    #region Movement
    void HandleMovement(ref FirstPersonPlayerCharacterUpdateData updateData)
    {
        UpdateRotation(m_Camera.transform.rotation);
        CollisionChecks();
        UpdateTimers(updateData.DeltaTime);
        UpdateMovementSpeed(updateData.SprintPressedThisFrame);
        UpdateHorizontalVelocity(updateData.MovementInput);
        JumpChecks(updateData.JumpPressedThisFrame);
        UpdateVerticalVelocity(updateData.DeltaTime);
        updateData.IsSprinting =
            m_bIsSprinting // Pressing sprint
            && (m_bIsGrounded || m_CharacterSettings.CanSprintInAir) // On ground or allowed to sprint in air
            && (updateData.MovementInput.x > s_Epsilon || updateData.MovementInput.y > s_Epsilon); // Pressing movement keys
        m_CC.Move(m_Velocity * updateData.DeltaTime);
    }

    void UpdateRotation(Quaternion rotation)
    {
        Vector3 forward = Vector3.ProjectOnPlane(rotation * Vector3.forward, Vector3.up).normalized;
        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }

    #region Collision Checks
    void GroundCheck() => m_bIsGrounded = m_CC.isGrounded;

    void BumpedHeadCheck() => m_bBumpedHead = m_CC.collisionFlags.HasFlag(CollisionFlags.Above);

    void CollisionChecks()
    {
        GroundCheck();
        BumpedHeadCheck();
    }
    #endregion

    void UpdateTimers(float deltaTime)
    {
        m_CurrentJumpBufferTimer -= deltaTime;
        if (!m_bIsGrounded) m_CurrentCoyoteTimer -= deltaTime;
        else m_CurrentCoyoteTimer = 0f;
    }

    void UpdateMovementSpeed(bool bSprintPressedThisFrame)
    {
        m_bIsSprinting = ((m_bIsGrounded && !m_bIsFalling) || m_CharacterSettings.CanSprintInAir) && bSprintPressedThisFrame;
        m_MovementSpeed = m_bIsSprinting ? m_CharacterSettings.SprintSpeed : m_CharacterSettings.MovementSpeed;
    }

    void UpdateHorizontalVelocity(Vector2 movementInput)
    {
        float xIn = movementInput.x;
        float zIn = movementInput.y;
        Vector3 hIn = Vector3.ClampMagnitude(xIn * transform.right + zIn * transform.forward, 1f);
        hIn = hIn.sqrMagnitude > s_SqrEpsilon ? hIn : Vector3.zero;
        m_Velocity.x = hIn.x * m_MovementSpeed;
        m_Velocity.z = hIn.z * m_MovementSpeed;
    }

    #region Jumping
    void JumpChecks(bool bJumpPressedThisFrame)
    {
        if (bJumpPressedThisFrame) m_CurrentJumpBufferTimer = m_CharacterSettings.JumpBuffer;
        if (m_CurrentJumpBufferTimer > 0f && !m_bIsJumping && (m_bIsGrounded || m_CurrentCoyoteTimer > 0f)) InitiateJump();
        if ((m_bIsJumping || m_bIsFalling) && m_bIsGrounded && m_Velocity.y <= 0f)
        {
            m_bIsJumping = false;
            m_bIsFalling = false;
            m_Velocity.y = 0f;
        }
    }

    void InitiateJump()
    {
        m_bIsJumping = true;
        m_CurrentJumpBufferTimer = 0f;
        m_Velocity.y = m_CharacterSettings.InitialJumpVelocity;
    }
    #endregion

    #region Update Vertical Velocity
    void UpdateVerticalVelocityWhileJumping(float deltaTime)
    {
        if (m_bBumpedHead && m_Velocity.y > 0) m_Velocity.y = 0f;
        m_Velocity.y -= m_CharacterSettings.PosGravity * deltaTime;
        m_bIsFalling = m_Velocity.y < 0f;
    }

    void UpdateVerticalVelocityWhileFalling(float deltaTime)
    {
        m_bIsFalling = true;
        m_Velocity.y -= m_CharacterSettings.PosGravity * deltaTime;
    }

    void UpdateVerticalVelocity(float deltaTime)
    {
        if (m_bIsJumping) UpdateVerticalVelocityWhileJumping(deltaTime);
        else if (!m_bIsGrounded) UpdateVerticalVelocityWhileFalling(deltaTime);
        else m_Velocity.y = -1f;
    }
    #endregion Update Vertical Velocity
    #endregion Movement

    #region Weapons
    enum WeaponAction { Shoot, Reload, Melee, SwitchWeapon }

    void HandleWeapons(ref FirstPersonPlayerCharacterUpdateData updateData)
    {
        if (updateData.ShootReleased && m_WeaponComponent.CurrentGun is Rifle rifle) rifle.StopAttacking();
        if (!m_bMeleeing) ResolveWeaponActions(ref updateData);
        updateData.ClearWeaponRequests();
    }

    void ResolveWeaponActions(ref FirstPersonPlayerCharacterUpdateData updateData)
    {
        bool bAnyStarted = false;
        bool bHandled = false;
        bool bOnlyReloadLeft = false;
        long limit = long.MaxValue;
        while (!bHandled && NextWeaponAction(ref updateData, limit, out WeaponAction action, out long order))
        {
            bAnyStarted = true;
            limit = order;
            if (bOnlyReloadLeft && action != WeaponAction.Reload) continue;
            switch (action)
            {
                case WeaponAction.Shoot:
                    bHandled = TryShoot(ref updateData);
                    break;
                case WeaponAction.Reload:
                    TryReload();
                    bHandled = true;
                    break;
                case WeaponAction.Melee:
                    bHandled = TryMelee();
                    bOnlyReloadLeft = !bHandled;
                    break;
                case WeaponAction.SwitchWeapon:
                    TrySwitchWeapon(ref updateData);
                    bHandled = true;
                    break;
            }
        }
        if (!bAnyStarted && updateData.ShootHeld && m_WeaponComponent.CurrentGun && m_WeaponComponent.CurrentGun.IsRapidFire)
            TryShoot(ref updateData);
    }

    bool NextWeaponAction(ref FirstPersonPlayerCharacterUpdateData updateData, long limit, out WeaponAction action, out long order)
    {
        action = default;
        order = 0;
        if (IsNewer(updateData.ShootStartedOrder, order, limit)) { action = WeaponAction.Shoot; order = updateData.ShootStartedOrder; }
        if (IsNewer(updateData.ReloadOrder, order, limit)) { action = WeaponAction.Reload; order = updateData.ReloadOrder; }
        if (IsNewer(updateData.MeleeOrder, order, limit)) { action = WeaponAction.Melee; order = updateData.MeleeOrder; }
        if (IsNewer(updateData.EquipOrder, order, limit)) { action = WeaponAction.SwitchWeapon; order = updateData.EquipOrder; }
        return order > 0;
    }

    static bool IsNewer(long candidate, long best, long limit) => candidate > 0 && candidate < limit && candidate > best;

    bool TryShoot(ref FirstPersonPlayerCharacterUpdateData updateData)
    {
        Gun gun = m_WeaponComponent.CurrentGun;
        if (!gun || !m_WeaponComponent.Shoot(CameraTarget)) return false;
        CancelReload();
        updateData.DoRecoil = true;
        updateData.RecoilAmount = gun.Recoil;
        string attackAnim = AttackAnimation(gun);
        if (!string.IsNullOrEmpty(attackAnim)) m_ArmsAnimator.Play(attackAnim, -1, 0f);
        return true;
    }

    void TryReload()
    {
        Gun gun = m_WeaponComponent.CurrentGun;
        if (!gun || gun.MagazineFull || gun.ReserveAmmo <= 0 || m_bReloading) return;
        string reloadAnim = ReloadAnimation(gun);
        IEnumerator reloadEndCheck = ReloadCheckEnd(this, gun);
        if (string.IsNullOrEmpty(reloadAnim) || reloadEndCheck == null) return;
        m_ArmsAnimator.Play(reloadAnim, -1, 0f);
        m_bReloading = true;
        StartCoroutine(reloadEndCheck);
        if (gun is Rifle rifle) rifle.StopAttacking();
    }

    bool TryMelee()
    {
        MeleeWeapon melee = m_WeaponComponent.CurrentMeleeWeapon;
        if (!melee || !m_WeaponComponent.MeleeAttack(CameraTarget)) return false;
        CancelReload(); // Melee cancels reload as to not lock player into defenceless state
        if (m_WeaponComponent.CurrentGun is Rifle rifle) rifle.StopAttacking();
        string attackAnim = AttackAnimation(melee);
        if (!string.IsNullOrEmpty(attackAnim))
        {
            m_ArmsAnimator.Play(attackAnim, -1, 0f);
            m_bMeleeing = true;
            StartCoroutine(CheckForMeleeAttackEnd(attackAnim));
        }
        return true;
    }

    void TrySwitchWeapon(ref FirstPersonPlayerCharacterUpdateData updateData)
    {
        if (updateData.EquipTarget != null && m_WeaponComponent.Equip(updateData.EquipTarget)) CancelReload();
    }

    void CancelReload()
    {
        if (!m_bReloading) return;
        m_bReloading = false;
        m_bReloadCancelled = true;
    }
    #endregion Weapons

    #region Animations
    IEnumerator CheckForPistolReloadEnd()
    {
        AnimatorStateInfo stateInfo = m_ArmsAnimator.GetCurrentAnimatorStateInfo(0);
        while (m_WeaponComponent.CurrentGun
            && m_bReloading
            && m_WeaponComponent.CurrentGun is Pistol
            && stateInfo.IsName("PistolReload") && stateInfo.normalizedTime < 1.0f)
        {
            stateInfo = m_ArmsAnimator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }
        m_bReloading = false;
        if (m_bReloadCancelled) m_bReloadCancelled = false;
        else if (m_WeaponComponent.CurrentGun is Pistol pistol) pistol.Reload();
    }

    IEnumerator CheckForRifleReloadEnd()
    {
        AnimatorStateInfo stateInfo = m_ArmsAnimator.GetCurrentAnimatorStateInfo(0);
        while (m_WeaponComponent.CurrentGun
            && m_bReloading
            && m_WeaponComponent.CurrentGun is Rifle
            && stateInfo.IsName("RifleReload") && stateInfo.normalizedTime < 1.0f)
        {
            stateInfo = m_ArmsAnimator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }
        m_bReloading = false;
        if (m_bReloadCancelled) m_bReloadCancelled = false;
        else if (m_WeaponComponent.CurrentGun is Rifle rifle) rifle.Reload();
    }

    IEnumerator CheckForGrenadeLauncherReloadEnd()
    {
        AnimatorStateInfo stateInfo = m_ArmsAnimator.GetCurrentAnimatorStateInfo(0);
        while (m_WeaponComponent.CurrentGun
            && m_bReloading
            && m_WeaponComponent.CurrentGun is GrenadeLauncher
            && stateInfo.IsName("GrenadeLauncherReload") && stateInfo.normalizedTime < 1.0f)
        {
            stateInfo = m_ArmsAnimator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }
        m_bReloading = false;
        if (m_bReloadCancelled) m_bReloadCancelled = false;
        else if (m_WeaponComponent.CurrentGun is GrenadeLauncher grenadeLauncher) grenadeLauncher.Reload();
    }

    IEnumerator CheckForShotgunReloadEnd()
    {
        AnimatorStateInfo stateInfo = m_ArmsAnimator.GetCurrentAnimatorStateInfo(0);
        while (m_WeaponComponent.CurrentGun
            && m_bReloading
            && m_WeaponComponent.CurrentGun is Shotgun
            && stateInfo.IsName("ShotgunReload") && stateInfo.normalizedTime < 1.0f)
        {
            stateInfo = m_ArmsAnimator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }
        m_bReloading = false;
        if (m_bReloadCancelled) m_bReloadCancelled = false;
        else if (m_WeaponComponent.CurrentGun is Shotgun shotgun) shotgun.Reload();
    }

    IEnumerator CheckForMeleeAttackEnd(string attackAnim)
    {
        int framesToEnter = 2;
        bool bEntered = false;
        while (true)
        {
            AnimatorStateInfo stateInfo = m_ArmsAnimator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName(attackAnim))
            {
                bEntered = true;
                if (stateInfo.normalizedTime >= 1.0f) break;
            }
            else if (bEntered || --framesToEnter < 0) break;
            yield return null;
        }
        m_bMeleeing = false;
    }
    #endregion
}
