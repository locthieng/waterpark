using System;
using System.Collections.Generic;
//using UnityEditor.Overlays;
using UnityEngine;

[Serializable]
public class BoosterData : ISerializationCallbackReceiver //
{
    [SerializeField] private List<BoosterType> _keys = new List<BoosterType>();
    [SerializeField] private List<int> _values = new List<int>();

    [NonSerialized]
    public Dictionary<BoosterType, int> BoosterDic = new Dictionary<BoosterType, int>();

    // --- LOGIC GAMEPLAY (Chỉ quan tâm Dictionary) ---

    public int GetCount(BoosterType type)
    {
        return BoosterDic.TryGetValue(type, out int count) ? count : 0;
    }

    public void AddBooster(BoosterType type, int amount = 1)
    {
        if (BoosterDic.ContainsKey(type))
            BoosterDic[type] += amount;
        else
            BoosterDic[type] = amount;
        BoosterDataController.Instance.SaveData();
    }

    public void AddAllBoosters(int num)
    {
        var keys = new List<BoosterType>(BoosterDic.Keys);

        foreach (var type in keys)
        {
            BoosterDic[type] += num;
        }

        BoosterDataController.Instance.SaveData();
    }

    public void RemoveBooster(BoosterType type, int amount = 1)
    {
        if (BoosterDic.ContainsKey(type))
        {
            BoosterDic[type] = Mathf.Max(BoosterDic[type] - amount, 0);
            BoosterDataController.Instance.SaveData();
        }
    }

    // --- LOGIC SAVE/LOAD RIÊNG ---

    const string IntroducedKeyPrefix = "Booster_Intro_";
    public bool IsIntroduced(BoosterType type) => PlayerPrefs.GetInt(IntroducedKeyPrefix + (int)type, 0) == 1;

    public void SetIntroduced(BoosterType type)
    {
        PlayerPrefs.SetInt(IntroducedKeyPrefix + (int)type, 1);
    }

    // --- MAGIC CỦA UNITY (ISerializationCallbackReceiver) ---

    public void OnBeforeSerialize()
    {
        _keys.Clear();
        _values.Clear();
        foreach (var kvp in BoosterDic)
        {
            _keys.Add(kvp.Key);
            _values.Add(kvp.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        BoosterDic = new Dictionary<BoosterType, int>();
        for (int i = 0; i != Math.Min(_keys.Count, _values.Count); i++)
        {
            if (!BoosterDic.ContainsKey(_keys[i]))
                BoosterDic.Add(_keys[i], _values[i]);
        }
    }
}

public class BoosterDataController : MonoBehaviour
{
    public static BoosterDataController Instance;
    private const string BoosterDataKey = "BoosterData";
    public BoosterData Data;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadData();
    }

    private void LoadData()
    {
        string json = PlayerPrefs.GetString(BoosterDataKey, "");

        if (string.IsNullOrEmpty(json))
        {
            Data = new BoosterData();
            foreach (BoosterType type in Enum.GetValues(typeof(BoosterType)))
            {
                Data.BoosterDic.Add(type, 0);
            }
        }
        else
        {
            Data = JsonUtility.FromJson<BoosterData>(json);
        }
    }

    public void SaveData()
    {
        string json = JsonUtility.ToJson(Data);
        PlayerPrefs.SetString(BoosterDataKey, json);
        PlayerPrefs.Save();

    }
}