
using QFramework;

namespace VirtualGameframe.BusinessLayer.Model.Item
{
    public class ItemBase : IModel
    {
        private int _itemID;
        private string _itemName;
        private string _itemDescription;
        private string _iconUrl;
        private string _itemQuality;
        private string _itemPrice;

        public ItemBase()
        {
            
        }

        public override string ToString()
        {
            return _itemName;
        }

        public void SetArchitecture(IArchitecture architecture)
        {
            throw new System.NotImplementedException();
        }

        public IArchitecture GetArchitecture()
        {
            throw new System.NotImplementedException();
        }

        public bool Initialized { get; set; }
        public void Init()
        {
            throw new System.NotImplementedException();
        }

        public void Deinit()
        {
            throw new System.NotImplementedException();
        }
    }
}
