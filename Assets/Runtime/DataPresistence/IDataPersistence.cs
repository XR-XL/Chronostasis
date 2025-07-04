using UnityEngine;

public interface IDataPersistence
{
    void LoadData(PlayerData data);
    void SaveData(ref PlayerData data);
}
