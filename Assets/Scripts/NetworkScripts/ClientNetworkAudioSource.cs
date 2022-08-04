using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ClientNetworkAudioSource : NetworkBehaviour
{
    [SerializeField] private AudioSource m_AudioSourceForStep;
    [SerializeField ] private AudioSource m_AudioSourceForSoundEffect;
    private PlayerController m_PlayerController;
    private Rigidbody m_Rigidbody;

    [SerializeField] AudioClip[] clips = new AudioClip[5];

    void Start()
    {
        //リジッドボディ取得
        m_Rigidbody = GetComponent<Rigidbody>();

        //プレーヤーコントローラー取得
        m_PlayerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if(!m_PlayerController.isGround)
        {
            m_AudioSourceForStep.volume = 0f;
        } else
        {
            float value = Mathf.Clamp(m_Rigidbody.velocity.magnitude, 0f, 7.5f) / 7.5f;
            m_AudioSourceForStep.pitch = value + 1.5f;
            if (IsOwner) value *= 0.1f;
            m_AudioSourceForStep.volume = value;
        }
    }

    public void PlayAudio(int audioNum)
    {
        m_AudioSourceForSoundEffect.PlayOneShot(clips[audioNum]);
        PlayerAudioServerRpc(audioNum);
    }

    [ServerRpc(RequireOwnership = false)] private void PlayerAudioServerRpc(int audioNum)
    {
        PlayerAudioClientRpc(audioNum);
    }
    [ClientRpc] private void PlayerAudioClientRpc(int audioNum)
    {
        if (IsOwner) return;
        m_AudioSourceForSoundEffect.PlayOneShot(clips[audioNum]);
    }
}
