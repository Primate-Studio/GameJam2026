using UnityEngine;


[CreateAssetMenu(fileName = "New Pack", menuName = "Shop/Pack")]
public class PackData : ScriptableObject
{
    public string packName;

    public ActivityData[] activities = new ActivityData[4];
}
    