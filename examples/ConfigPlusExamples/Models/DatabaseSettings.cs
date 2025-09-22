using System.ComponentModel.DataAnnotations;

namespace ConfigPlusExamples.Models
{
    /// <summary>
    /// Database bağlantı ayarları
    /// </summary>
    public class DatabaseSettings
    {
        [Required(ErrorMessage = "Connection string zorunludur")]
        public string ConnectionString { get; set; } = string.Empty;

        [Range(1, 3600, ErrorMessage = "Timeout 1-3600 saniye arasında olmalı")]
        public int CommandTimeoutSeconds { get; set; } = 30;

        public bool EnableRetryPolicy { get; set; } = true;

        [Range(1, 10, ErrorMessage = "Retry sayısı 1-10 arasında olmalı")]
        public int MaxRetryAttempts { get; set; } = 3;
    }
}
