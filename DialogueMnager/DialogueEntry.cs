//�����N�Ω�s�x�q Excel ��Ū�����ƾڡC
[System.Serializable]
public class DialogueEntry 
{
    public int diaID;//��ܪ��D�s���A�i�ѥ~���{���I�s������w���
    public int diaScript;//�l�s���Ʀr�A�i�Ω��{������
    public string diaChrImgHightlight;//���G����Ϥ��A1�����ϡB2���k�ϡB3�����k�@�_�G
    public string diaChrNameL;//����ܨ��⪺�W�r�A�Ω���ܦb�~����ܭ��O�����H���W��
    public string diaChrImgL;//����ܨ��⪺�Ϥ��A�Ω���ܦb�~����ܭ��O�����H���Ϥ�
    public string diaChrNameR;//�k��ܨ��⪺�W�r�A�Ω���ܦb�~����ܭ��O�k���H���W��
    public string diaChrImgR;//�k��ܨ��⪺�Ϥ��A�Ω���ܦb�~����ܭ��O�k���H���Ϥ�
    public string diaText;//��ܤ��e�Ω���ܦb�~����ܭ��O�W
    public string diaTextEffect; //�Ϲ�ܮؤ�r���X���w�s�����S��ĪG
    public string diaSelection; //��ܿﶵ�A�Ω��ܹ�ܤ���
    public string diaConditions;//��ܺ�������A������������~��ϸӹ�ܯ�Q�襤�����
    public string diaEffects;//�ݭn�i�X�i���J"�ܼƦW��"�B"�ܼƼƭ�"�A���ܳQ����ο�ܮɡA�������ƭȱN�o���ܤơC
    public string diaImgBackground;//�������w�s�����I���Ϥ�
    public string diaImgBackgroundEffects; //�ϭI���Ϥ����X���w�s�����S��ĪG
    public string diaBackgroundMusic;//�Ӭq���i��ɼ�����w�s���I������
    public string diaSoundEffect;//�Ӭq���i��ɼ�����w�s������
    public string nextDiaID;//�Ϲ�ܵ�����������wdiaID�A�p��ܬ�END�h����������
}
