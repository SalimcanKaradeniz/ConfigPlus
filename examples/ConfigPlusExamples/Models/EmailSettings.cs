using System.ComponentModel.DataAnnotations;

namespace ConfigPlusExamples.Models
{
    /// <summary>
    /// Email servisi ayarları
    /// </summary>
    public class EmailSettings
    {
        [Required(ErrorMessage = "SMTP host adresi zorunludur")]
        public string SmtpHost { get; set; } = string.Empty;

        [Range(1, 65535, ErrorMessage = "SMTP port 1-65535 arasında olmalı")]
        public int SmtpPort { get; set; } = 587;

        [Required(ErrorMessage = "Gönderici email adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli email formatında olmalı")]
        public string FromAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gönderici adı zorunludur")]
        public string FromName { get; set; } = string.Empty;

        public bool EnableSsl { get; set; } = true;
    }
}
