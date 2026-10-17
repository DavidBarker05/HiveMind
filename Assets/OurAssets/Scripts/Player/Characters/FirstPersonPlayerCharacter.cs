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

    public bool ShootPressedThisFrame { get; set; }
    public bool ShootReleasedThisFrame { get; set; }

    public bool MeleeAttackStarted { get; set; }

    public bool ReloadStarted { get; set; }
    #endregion Input

    #region Output
    public bool IsSprinting { get; set; }

    public bool DoRecoil { get; set; }
    public float RecoilAmount { get; set; }
    #endregion
}

[RequireComponent(typeof(CharacterController), typeof(WeaponHandlerComponent))]
public class FirstPersonPlayerCharacter : PlayerCharacter
{
    static readonly float s_Epsilon = 0.05f;
    static readonly float s_SqrEpsilon = s_Epsilon * s_Epsilon;

    static string AttackAnimation(Weapon weapon) => weapon switch
    {
        Pistol => "PistolShoot",
        Rifle => "RifleShoot",
        _ => string.Empty
    };

    static string ReloadAnimation(Gun gun) => gun switch
    {
        Pistol => "PistolReload",
        Rifle => "RifleReload",
        _ => string.Empty
    };

    static IEnumerator ReloadCheckEnd(FirstPersonPlayerCharacter self, Gun gun) => gun switch
    {
        Pistol => self.CheckForPistolReloadEnd(),
        Rifle => self.CheckForRifleReloadEnd(),
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

    bool m_bIsGrounded;
    bool m_bBumpedHead;

    Vector3 m_Velocity;
    bool m_bIsJumping;
    bool m_bIsFalling;

    float m_CurrentJumpBufferTimer;
    float m_CurrentCoyoteTimer;

    bool m_bIsSprinting;
    float m_MovementSpeed;

    bool m_bAttackPressedLastFrame = false;

    bool m_bReloading = false;

    public override bool HasBeenInitialised { get; protected set; }

    public override string ActionMap => "Player";
    public override bool MouseVisible => false;
    public override bool DoCameraRotation => true;

    PausePlayerCharacter m_PausePlayerCharacter;

    void Awake()
    {
        m_CC = GetComponent<CharacterController>();
        m_WeaponComponent = GetComponent<WeaponHandlerComponent>();
    }

    public override void Init(IPlayerCharacterInitData playerCharacterInitData)
    {
        FirstPersonPlayerCharacterInitData initData = Sys.AssertType<FirstPersonPlayerCharacterInitData>(playerCharacterInitData, nameof(playerCharacterInitData));
        m_CharacterSettings = initData.CharacterSettings;
        m_PausePlayerCharacter = initData.PausePlayerCharacter;
        m_Camera = initData.Camera;
        m_WeaponComponent.Init();
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
        updateData.IsSprinting = m_bIsSprinting;
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
    void HandleWeapons(ref FirstPersonPlayerCharacterUpdateData updateData)
    {
        HandleGun(ref updateData);
        HandleReload(ref updateData);
        HandleMelee(ref updateData);
    }

    void HandleGun(ref FirstPersonPlayerCharacterUpdateData updateData)
    {
        if (!m_WeaponComponent.CurrentGun
            || updateData.MeleeAttackStarted
            || updateData.ReloadStarted
            || m_bReloading) return;
        updateData.DoRecoil = HandleShoot(updateData.ShootPressedThisFrame);
        if (updateData.DoRecoil)
        {
            updateData.RecoilAmount = m_WeaponComponent.CurrentGun.Recoil;
            string attackAnim = AttackAnimation(m_WeaponComponent.CurrentGun);
            if (!string.IsNullOrEmpty(attackAnim))
                m_ArmsAnimator.Play(attackAnim, -1, 0f);
        }
        m_bAttackPressedLastFrame = updateData.ShootPressedThisFrame && !updateData.ShootReleasedThisFrame;
        if (m_WeaponComponent.CurrentGun is Rifle rifle && updateData.ShootReleasedThisFrame)
            rifle.StopAttacking();
    }

    bool HandleShoot(bool bAttackPressedThisFrame)
    {
        if (bAttackPressedThisFrame && (!m_bAttackPressedLastFrame || m_WeaponComponent.CurrentGun.IsRapidFire))
            return m_WeaponComponent.Shoot(CameraTarget);
        return false;
    }

    void HandleReload(ref FirstPersonPlayerCharacterUpdateData updateData)
    {
        if (!m_WeaponComponent.CurrentGun
            || m_WeaponComponent.CurrentGun.MagazineFull
            || m_bReloading)
        {
            updateData.ReloadStarted = false;
            return;
        }
        if (!updateData.ReloadStarted) return;
        string reloadAnim = ReloadAnimation(m_WeaponComponent.CurrentGun);
        IEnumerator reloadEndCheck = ReloadCheckEnd(this, m_WeaponComponent.CurrentGun);
        if (!string.IsNullOrEmpty(reloadAnim) && reloadEndCheck != null)
        {
            m_ArmsAnimator.Play(reloadAnim, -1, 0f);
            m_bReloading = true;
            StartCoroutine(reloadEndCheck);
            if (m_WeaponComponent.CurrentGun is Rifle rifle)
                rifle.StopAttacking();
        }
        updateData.ReloadStarted = false;
    }

    void HandleMelee(ref FirstPersonPlayerCharacterUpdateData updateData)
    {
        if (!m_WeaponComponent.CurrentMeleeWeapon)
        {
            updateData.MeleeAttackStarted = false;
            return;
        }
        if (HandleMeleeAttack(updateData.MeleeAttackStarted))
        {
            string attackAnim = AttackAnimation(m_WeaponComponent.CurrentMeleeWeapon);
            if (!string.IsNullOrEmpty(attackAnim))
            {
                m_ArmsAnimator.Play(attackAnim, -1, 0f);
                m_bReloading = false; // Melee cancels reload as to not lock player into defenceless state
            }
        }
        updateData.MeleeAttackStarted = false;
    }

    bool HandleMeleeAttack(bool bAttackPressedThisFrame)
    {
        if (bAttackPressedThisFrame) return m_WeaponComponent.MeleeAttack(CameraTarget);
        return false;
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
        if (m_WeaponComponent.CurrentGun is Pistol pistol) pistol.Reload();
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
        if (m_WeaponComponent.CurrentGun is Rifle rifle) rifle.Reload();
    }
    #endregion
}
