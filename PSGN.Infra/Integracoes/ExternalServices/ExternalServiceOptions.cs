namespace PSGN.Infra.Integracoes.ExternoServices
{
    public class ExternalServiceOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public int ParentCategoryId { get; set; }
    }
}
