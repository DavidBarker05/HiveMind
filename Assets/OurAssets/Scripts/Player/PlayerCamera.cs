using UnityEngine;

public struct CameraInitData
{
    public Camera Camera { get; set; }
    public CameraSettings CameraSettings { get; set; }
}

public struct CameraUpdateData
{
    public float DeltaTime { get; set; }
    public bool DoCameraRotation { get; set; }
    public Vector2 LookInput { get; set; }

    public bool DoRecoil { get; set; }
}

public struct CameraLateUpdateData
{
    public float DeltaTime { get; set; }

    public Transform Target { get; set; }

    public bool IsSprinting { get; set; }

    public bool DoRecoil { get; set; }
    public float RecoilAmount { get; set; }
}

public class PlayerCamera : MonoBehaviour
{
    Camera m_Camera;
    CameraSettings m_CameraSettings;
    Vector3 m_EulerAngles;

    float m_PitchWhenShot;
    float m_RecoilPitch;
    float m_LastAppliedPitch;
    float m_CurrentRecoilTime;

    public void Init(CameraInitData initData)
    {
        m_Camera = initData.Camera;
        m_CameraSettings = initData.CameraSettings;
        m_CurrentRecoilTime = m_CameraSettings.RecoilDuration;
    }

    public void ChangeCameraTarget(Transform target)
    {
        transform.position = target.position;
        transform.eulerAngles = target.eulerAngles;
        m_EulerAngles = target.eulerAngles;
    }

    public void SetRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
        m_EulerAngles = rotation.eulerAngles;
    }

    public void UpdateCamera(ref CameraUpdateData updateData)
    {
        UpdateCameraFromInput(updateData.LookInput);
        if (updateData.DoRecoil) UpdateCameraFromRecoil(updateData.DeltaTime);
        transform.eulerAngles = m_EulerAngles;
    }

    void UpdateCameraFromInput(Vector2 lookInput)
    {
        float lY = lookInput.y;
        float lX = lookInput.x;
        float vSens = m_CameraSettings.VerticalSensitivity;
        float hSens = m_CameraSettings.HorizontalSensitivity;
        float pitch = lY * vSens;
        float yaw = lX * hSens;
        m_EulerAngles.x = Mathf.Clamp(m_EulerAngles.x - pitch, m_CameraSettings.MinVerticalAngle, m_CameraSettings.MaxVerticalAngle);
        m_EulerAngles.y += yaw;
    }

    void UpdateCameraFromRecoil(float deltaTime)
    {
        if (m_CurrentRecoilTime >= m_CameraSettings.RecoilDuration) return;
        m_CurrentRecoilTime += deltaTime;
        float alpha = Mathf.Clamp(m_CurrentRecoilTime / m_CameraSettings.RecoilDuration, 0f, 1f);
        float desiredPitch = Mathf.Lerp(m_PitchWhenShot, m_RecoilPitch, alpha);
        float deltaPitch = desiredPitch - m_LastAppliedPitch;
        m_EulerAngles.x = Mathf.Clamp(m_EulerAngles.x - deltaPitch, m_CameraSettings.MinVerticalAngle, m_CameraSettings.MaxVerticalAngle);
        m_LastAppliedPitch = desiredPitch;
    }

    public void ResetFOV() => m_Camera.fieldOfView = m_CameraSettings.VerticalFieldOfView;

    public void LateUpdateCamera(ref CameraLateUpdateData lateUpdateData)
    {
        UpdatePosition(lateUpdateData.Target);
        UpdateCameraFOV(lateUpdateData.IsSprinting, lateUpdateData.DeltaTime);
        if (lateUpdateData.DoRecoil) StartRecoil(lateUpdateData.RecoilAmount);
    }

    void UpdatePosition(Transform target) => transform.position = target.position;

    void UpdateCameraFOV(bool bIsSprinting, float deltaTime)
    {
        float targetFOV = bIsSprinting ? m_CameraSettings.SprintVerticalFieldOfView : m_CameraSettings.VerticalFieldOfView;
        float fovRange = Mathf.Abs(m_CameraSettings.SprintVerticalFieldOfView - m_CameraSettings.VerticalFieldOfView);
        float transitionSpeed = m_CameraSettings.FieldOfViewTransitionDuration > 0f ? fovRange / m_CameraSettings.FieldOfViewTransitionDuration : float.MaxValue;
        m_Camera.fieldOfView = Mathf.MoveTowards(m_Camera.fieldOfView, targetFOV, transitionSpeed * deltaTime);
    }

    void StartRecoil(float recoilAmount)
    {
        m_PitchWhenShot = m_EulerAngles.x;
        m_RecoilPitch = m_PitchWhenShot + recoilAmount;
        m_LastAppliedPitch = m_PitchWhenShot;
        m_CurrentRecoilTime = 0f;
    }
}