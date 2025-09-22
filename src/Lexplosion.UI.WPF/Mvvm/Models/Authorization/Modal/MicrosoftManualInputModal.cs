using Lexplosion.UI.WPF.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;

namespace Lexplosion.UI.WPF.Mvvm.Models.Authorization.Modal
{
    public sealed class MicrosoftManualInputModel : ViewModelBase
    {
        private readonly AppCore _appCore;

        private string _microsoftToken;
        public string MicrosoftToken
        {
            get => _microsoftToken; set
            {
                _microsoftToken = value;
                IsTokenValid = IsRightMicrosoftToken(value);
                OnPropertyChanged(nameof(IsTokenValid));
            }
        }

        public bool IsTokenValid { get; private set; }

        public MicrosoftManualInputModel(AppCore appCore)
        {
            _appCore = appCore;
        }

        bool IsJwt(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            var parts = token.Split('.');

            if (parts.Length != 3)
            {
                return false;
            }

            try
            {
                // first part of jwt - json
                var jsonFirstPartBytes = Convert.FromBase64String(parts[0]);
                var jsonFirstPart = System.Text.Encoding.UTF8.GetString(jsonFirstPartBytes);

                var result1 = JsonConvert.DeserializeObject(jsonFirstPart);

                // second part of jwt - json
                var jsonSecondPartBytes = Convert.FromBase64String(parts[1] + "==");
                var jsonSecondPart = System.Text.Encoding.UTF8.GetString(jsonSecondPartBytes);

                var result2 = JsonConvert.DeserializeObject(jsonSecondPart);
            }
            catch
            {
                return false;
            }

            return true;
        }

        struct MicrosoftAuthData 
        {
            [JsonProperty("uhs")]
            public string Uhs { get; set; }

            [JsonProperty("xsts_token")]
            public string XstsToken { get; set; }
        }

        bool IsRightMicrosoftToken(string token) 
        {
            try
            {
                var microsoftAuthData = JsonConvert.DeserializeObject<MicrosoftAuthData>(token);

                if (microsoftAuthData.Uhs.Length == 0 || microsoftAuthData.XstsToken.Length == 0)
                    return false;

                return true;
            }
            catch 
            {
                return false;
            }
        }
    }
}
