using UnityEngine;

public class UISoundManager : MonoBehaviour
{
    public static UISoundManager Instance { get; private set; }

    [SerializeField] private UISoundDefinitions soundDefinitions;
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlaySound(UISoundDefinitions.UIEvent uiEvent)
    {
        foreach (var mapping in soundDefinitions.sounds)
        {
            if (mapping.uiEvent == uiEvent)
            {
                audioSource.PlayOneShot(mapping.audioClip);
                break;
            }
        }
    }
}
