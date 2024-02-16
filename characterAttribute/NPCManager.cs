using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public NPCAttribute[] npcs;
    private Dictionary<string, NPCAttribute> npcDictionary;
    void Start()
    {
        // 初始化 NPC 字典
        npcDictionary = new Dictionary<string, NPCAttribute>();
        for (int i = 0; i < npcs.Length; i++)
        {
            // 使用索引作为 ID
            string npcId = "NPC" + (i + 1); // 例如，NPC1、NPC2 等
            npcDictionary.Add(npcId, npcs[i]);
        }
    }

    public IAttributeChanger GetNPCAttributeChanger(string npcId)
    {
        if (npcDictionary.TryGetValue(npcId, out NPCAttribute npc))
        {
            return npc;
        }
        else
        {
            Debug.LogError($"NPC with ID {npcId} not found.");
            return null;
        }
    }
}