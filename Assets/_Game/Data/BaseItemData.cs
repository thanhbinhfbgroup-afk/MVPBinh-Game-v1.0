using UnityEngine;
namespace BillGameCore.Data
{
    public abstract class BaseItemData : ScriptableObject
    {
        public string Id;
        public string Name;
        public Sprite Icon;
        [TextArea] public string Description;
    }
}