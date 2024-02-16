//此類將用於存儲從 Excel 檔讀取的數據。
[System.Serializable]
public class DialogueEntry 
{
    public int diaID;//對話的主編號，可由外部程式呼叫播放指定對話
    public int diaScript;//子編號數字，可用於辨認分支對話
    public string diaChrImgHightlight;//高亮角色圖片，1為左圖、2為右圖、3為左右一起亮
    public string diaChrNameL;//左對話角色的名字，用於顯示在外部對話面板左側人物名稱
    public string diaChrImgL;//左對話角色的圖片，用於顯示在外部對話面板左側人物圖片
    public string diaChrNameR;//右對話角色的名字，用於顯示在外部對話面板右側人物名稱
    public string diaChrImgR;//右對話角色的圖片，用於顯示在外部對話面板右側人物圖片
    public string diaText;//對話內容用於顯示在外部對話面板上
    public string diaTextEffect; //使對話框文字做出指定編號的特殊效果
    public string diaSelection; //對話選項，用於選擇對話分支
    public string diaConditions;//對話滿足條件，必須滿足條件才能使該對話能被選中或顯示
    public string diaEffects;//需要可擴展能輸入"變數名稱"、"變數數值"，當對話被執行或選擇時，相應的數值將發生變化。
    public string diaImgBackground;//切換指定編號的背景圖片
    public string diaImgBackgroundEffects; //使背景圖片做出指定編號的特殊效果
    public string diaBackgroundMusic;//該段落進行時播放指定編號背景音樂
    public string diaSoundEffect;//該段落進行時播放指定編號音效
    public string nextDiaID;//使對話結束後跳轉到指定diaID，如顯示為END則結束整場對話
}
