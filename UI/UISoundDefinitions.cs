using UnityEngine;

[CreateAssetMenu(fileName = "UiSoundDefinitions", menuName = "Audio/UI Sound Definitions", order = 1)]
public class UISoundDefinitions : ScriptableObject
{
    public SoundMapping[] sounds;

    [System.Serializable]
    public class SoundMapping
    {
        public UIEvent uiEvent;
        public AudioClip audioClip;
    }

    public enum UIEvent
    {
        MouseEnter,
        MousePress,
        MouseRelease
    }
}
