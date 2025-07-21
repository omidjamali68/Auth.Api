namespace Auth.Api.Settings
{
    public class IrSmsSetting
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string LoginPaternCode { get; set; } = string.Empty;
    }

    public class SmsParams
    {
        public SmsParams(string Key, string Value)
        {
            this.Key = Key;
            this.Value = Value;
        }

        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
