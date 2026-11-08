using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    [SerializeField]
    PlayerSettings m_PlayerSettings;
    [SerializeField]
    PlayerCharacter m_StartingPlayerCharacter;
    [SerializeField]
    PausePlayerCharacter m_PausePlayerCharacter;
    [SerializeField]
    MenuPlayerCharacter m_MenuPlayerCharacter;
    [SerializeField]
    PlayerCamera m_PlayerCamera;
    [SerializeField]
    Camera m_Camera;

    PlayerInput m_PlayerInput;

    PlayerCharacter m_PlayerCharacter;
    IPlayerCharacterUpdateData m_PlayerCharacterUpdateData;
    CameraUpdateData m_CameraUpdateData;
    CameraLateUpdateData m_CameraLateUpdateData;

    public PlayerCharacter CurrentPlayerCharacter => m_PlayerCharacter;

    void Awake()
    {
        m_PlayerInput = GetComponent<PlayerInput>();
        m_PlayerCamera.Init(new CameraInitData()
        {
            Camera = m_Camera,
            CameraSettings = m_PlayerSettings.CameraSettings
        });
        m_PausePlayerCharacter.Init(new PausePlayerCharacterInitData() { Player = this });
        m_MenuPlayerCharacter.Init(new MenuPlayerCharacterInitData() { Player = this });
        ChangeCharacter(m_StartingPlayerCharacter);
        m_CameraUpdateData = new CameraUpdateData();
        m_CameraLateUpdateData = new CameraLateUpdateData();
    }

    void Update()
    {
        if (!m_PlayerCharacter || !m_PlayerCamera) return;
        SetCursorVisibility(m_PlayerCharacter.MouseVisible);
        m_PlayerCharacterUpdateData.DeltaTime = Time.deltaTime;
        m_CameraUpdateData.DeltaTime = Time.deltaTime;
        m_CameraUpdateData.DoRecoil = m_PlayerCharacter && m_PlayerCharacter is FirstPersonPlayerCharacter;
        m_CameraUpdateData.DoCameraRotation = m_PlayerCharacter.DoCameraRotation;
        m_PlayerCamera.UpdateCamera(ref m_CameraUpdateData);
        m_PlayerCharacter.UpdateCharacter(ref m_PlayerCharacterUpdateData);
        m_CameraLateUpdateData.DeltaTime = Time.deltaTime;
        if (m_PlayerCharacterUpdateData is FirstPersonPlayerCharacterUpdateData firstPersonPlayerCharacterUpdateData)
        {
            m_CameraLateUpdateData.IsSprinting = firstPersonPlayerCharacterUpdateData.IsSprinting;
            m_CameraLateUpdateData.DoRecoil = firstPersonPlayerCharacterUpdateData.DoRecoil;
            m_CameraLateUpdateData.RecoilAmount = firstPersonPlayerCharacterUpdateData.RecoilAmount;
        }
        else m_PlayerCamera.ResetFOV();
    }

    void LateUpdate()
    {
        m_CameraLateUpdateData.Target = m_PlayerCharacter.CameraTarget;
        m_PlayerCamera.LateUpdateCamera(ref m_CameraLateUpdateData);
        if (m_PlayerCharacterUpdateData is FirstPersonPlayerCharacterUpdateData firstPersonPlayerCharacterUpdateData)
            firstPersonPlayerCharacterUpdateData.DoRecoil = false;
    }

    #region Change Character
    public void ChangeCharacter(PlayerCharacter playerCharacter)
    {
        if (!playerCharacter) return;
        if (m_PlayerCharacter) m_PlayerCharacter.OnCharacterExit();
        m_PlayerCharacter = playerCharacter;
        if (!m_PlayerCharacter.HasBeenInitialised) m_PlayerCharacter.Init(PlayerCharacterInitData);
        m_PlayerCharacterUpdateData = PlayerCharacterUpdateData;
        if (!string.IsNullOrWhiteSpace(m_PlayerCharacter.ActionMap)) m_PlayerInput.SwitchCurrentActionMap(m_PlayerCharacter.ActionMap);
        m_PlayerCamera.ChangeCameraTarget(m_PlayerCharacter.CameraTarget);
        SetCursorVisibility(m_PlayerCharacter.MouseVisible);
        if (m_PlayerCharacter) m_PlayerCharacter.OnCharacterEnter();
    }

    IPlayerCharacterInitData PlayerCharacterInitData => m_PlayerCharacter switch
    {
        FirstPersonPlayerCharacter => new FirstPersonPlayerCharacterInitData()
        {
            Camera = m_Camera,
            CharacterSettings = m_PlayerSettings.CharacterSettings,
            PausePlayerCharacter = m_PausePlayerCharacter,
            MenuPlayerCharacter = m_MenuPlayerCharacter
        },
        PausePlayerCharacter => new PausePlayerCharacterInitData() { Player = this },
        MenuPlayerCharacter => new MenuPlayerCharacterInitData() { Player = this },
        _ => null
    };

    IPlayerCharacterUpdateData PlayerCharacterUpdateData => m_PlayerCharacter switch
    {
        FirstPersonPlayerCharacter => new FirstPersonPlayerCharacterUpdateData(),
        PausePlayerCharacter => new PausePlayerCharacterUpdateData(),
        MenuPlayerCharacter => new MenuPlayerCharacterUpdateData(),
        _ => null
    };
    #endregion Change Character

    #region Cursor Toggles
    public void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    public void HideCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SetCursorVisibility(bool bVisible)
    {
        if (bVisible) ShowCursor();
        else HideCursor();
    }
    #endregion Cursor Toggles

    #region Handle PlayerInput Events
    delegate void SetDataValueFunc<T>(T _) where T : class, IPlayerCharacterUpdateData;

    void SetDataValue<T>(SetDataValueFunc<T> dataChangeFunction) where T : class, IPlayerCharacterUpdateData
    {
        if (m_PlayerCharacterUpdateData is T t) dataChangeFunction(t);
    }

    public void HandlePauseInput(InputAction.CallbackContext _) => m_PlayerCharacter.OnPausePressed();

    public void HandleMoveInput(InputAction.CallbackContext ctx)
    {
        SetDataValue<FirstPersonPlayerCharacterUpdateData>(updateData => updateData.MovementInput = ctx.ReadValue<Vector2>());
    }

    public void HandleLookInput(InputAction.CallbackContext ctx)
    {
        if (!m_PlayerCharacter.DoCameraRotation) return;
        m_CameraUpdateData.LookInput = ctx.ReadValue<Vector2>();
    }

    public void HandleSprintInput(InputAction.CallbackContext ctx)
    {
        SetDataValue<FirstPersonPlayerCharacterUpdateData>(updateData => updateData.SprintPressedThisFrame = ctx.action.WasPressedThisFrame());
    }

    public void HandleJumpInput(InputAction.CallbackContext ctx)
    {
        SetDataValue<FirstPersonPlayerCharacterUpdateData>(updateData => updateData.JumpPressedThisFrame = ctx.action.WasPressedThisFrame());
    }

    // Weapon inputs record the order they arrived in so the character can tell which one was done last within a frame
    public void HandleShootInput(InputAction.CallbackContext ctx)
    {
        SetDataValue<FirstPersonPlayerCharacterUpdateData>(updateData =>
        {
            if (ctx.started)
            {
                updateData.ShootHeld = true;
                updateData.ShootStartedOrder = updateData.NextInputOrder();
            }
            else if (ctx.canceled)
            {
                updateData.ShootHeld = false;
                updateData.ShootReleased = true;
            }
        });
    }

    public void HandleMeleeInput(InputAction.CallbackContext ctx)
    {
        SetDataValue<FirstPersonPlayerCharacterUpdateData>(updateData =>
        {
            if (ctx.started) updateData.MeleeOrder = updateData.NextInputOrder();
        });
    }

    public void HandleReloadInput(InputAction.CallbackContext ctx)
    {
        SetDataValue<FirstPersonPlayerCharacterUpdateData>(updateData =>
        {
            if (ctx.started) updateData.ReloadOrder = updateData.NextInputOrder();
        });
    }

    public void HandleEquipPistolInput(InputAction.CallbackContext ctx)
    {
        SetDataValue<FirstPersonPlayerCharacterUpdateData>(updateData =>
        {
            if (ctx.started) updateData.RequestEquip<Pistol>();
        });
    }

    public void HandleEquipRifleInput(InputAction.CallbackContext ctx)
    {
        SetDataValue<FirstPersonPlayerCharacterUpdateData>(updateData =>
        {
            if (ctx.started) updateData.RequestEquip<Rifle>();
        });
    }

    public void HandleEquipGrenadeLauncherInput(InputAction.CallbackContext ctx)
    {
        SetDataValue<FirstPersonPlayerCharacterUpdateData>(updateData =>
        {
            if (ctx.started) updateData.RequestEquip<GrenadeLauncher>();
        });
    }

    public void HandleEquipShotgunInput(InputAction.CallbackContext ctx)
    {
        SetDataValue<FirstPersonPlayerCharacterUpdateData>(updateData =>
        {
            if (ctx.started) updateData.RequestEquip<Shotgun>();
        });
    }
    #endregion Handle PlayerInput Events
}