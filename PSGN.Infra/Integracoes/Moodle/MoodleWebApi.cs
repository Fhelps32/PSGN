using System.Text.Json;

namespace PSGN.Infra.Integracoes.Moodle
{
    // POST para o Web Service do Moodle com tratamento de erro único. Usado por todos os serviços do Moodle (cursos, módulos, usuários, tentativas)
    internal static class MoodleWebApi
    {
        public static async Task<T> ChamarAsync<T>(
            HttpClient httpClient,
            ConfigMoodleWebApi config,
            string funcao,
            Dictionary<string, string> parametros)
        {
            parametros["wstoken"] = config.Token;
            parametros["wsfunction"] = funcao;
            parametros["moodlewsrestformat"] = "json";

            var response = await httpClient.PostAsync(config.BaseUrl, new FormUrlEncodedContent(parametros));

            // Erros HTTP aqui; erros de negócio do Moodle chegam com 200 e são tratados abaixo
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            // Todo erro do Moodle tem a propriedade "exception"
            using var documento = JsonDocument.Parse(json);
            if (documento.RootElement.ValueKind == JsonValueKind.Object
                && documento.RootElement.TryGetProperty("exception", out _))
                throw new InvalidOperationException($"O Moodle retornou um erro na função {funcao}: {json}");

            return documento.RootElement.Deserialize<T>()
                ?? throw new InvalidOperationException($"O Moodle retornou uma resposta vazia na função {funcao}.");
        }
    }
}
