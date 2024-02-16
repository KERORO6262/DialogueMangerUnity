using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIInteractionSoundRegistrar : MonoBehaviour
{
    public Button[] buttons; // �i�H�b Inspector ����ʫ��w�ݭn���U���ʭ��Ī����s

    private void Awake()
    {
        // ���C�ӫ��s�K�[ UIInteractionSound �欰
        foreach (var button in buttons)
        {
            var sound = button.gameObject.AddComponent<UIInteractionSound>();
            // �p�G�ݭn�� sound �i���B�~�t�m�A�i�H�b�o�̾ާ@
        }
    }
}
