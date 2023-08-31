namespace FileSharing.Models
{
    public class SettingsModel
    {
        public SettingsModel()
        {
            Id = 0;
        }

        public int Id { get; set; } = 0;
        public string Key { get; set; }
        public bool Value { get; set; }
        public string Account_Id { get; set; } = string.Empty;
    }
}
