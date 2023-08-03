namespace NeonUI.ViewModels
{
    public class ComboBoxItem
    {
        public string Key { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public ComboBoxItem(string key, string name)
        {
            Key = key;
            Name = name;
        }
    }
}
