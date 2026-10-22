using System.Text.Json;

namespace PSGN.Infra.Integracoes.Moodle
{
    //REVISÃO: classe nova. Os 3 serviços do Moodle repetiam o mesmo código (montar token/função/formato, fazer o POST,
    //checar o status, ler o JSON) e cada um tratava erro de um jeito: um deles tinha a checagem comentada e outro nem tinha.
    //Agora todos passam por aqui e tratam erro do mesmo jeito.
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

            //REVISÃO: (sua pergunta "melhor forma de implementar isso?") O EnsureSuccessStatusCode está certo: ele cuida dos
            //erros HTTP (500, 404, 503...). O detalhe do Moodle é que os erros de NEGÓCIO (token inválido, função não
            //liberada, parâmetro errado) chegam com HTTP 200. Por isso existe a segunda checagem logo abaixo.
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            //REVISÃO: antes a checagem era "o JSON começa com {". Só que o core_course_get_courses_by_field devolve um objeto
            //({"courses": [...], "warnings": []}) mesmo quando dá certo, e por isso ela estava comentada no
            //ModuloMoodleSyncService. Todo erro do Moodle tem a propriedade "exception"; procurar por ela funciona com qualquer função.
            using var documento = JsonDocument.Parse(json);
            if (documento.RootElement.ValueKind == JsonValueKind.Object
                && documento.RootElement.TryGetProperty("exception", out _))
                throw new InvalidOperationException($"O Moodle retornou um erro na função {funcao}: {json}");

            return documento.RootElement.Deserialize<T>()
                ?? throw new InvalidOperationException($"O Moodle retornou uma resposta vazia na função {funcao}.");
        }
    }
}
