using System.ComponentModel.DataAnnotations;

namespace PSGN.Infra.Integracoes.Moodle
{
    public class ConfigMoodleWebApi
    {
        [Required]
        public string BaseUrl { get; init; } = string.Empty;

        [Required]
        public string Token { get; init; } = string.Empty;
    }
}