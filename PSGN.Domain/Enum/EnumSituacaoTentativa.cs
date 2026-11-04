namespace PSGN.Domain.Enum // mesma pasta "Enum" que o PSG usa para os enums do domínio
{
    // REVISÃO: enum novo, da regra de tentativas definida pelo usuário. NÃO é gravado no banco: é calculado pela Tentativa
    // (Tentativa.Situacao) a partir do que já existe nela: se está aberta (DataFimTentativa), se tem prova do Moodle (IdAttempt) e a nota.
    public enum EnumSituacaoTentativa // em que ponto do ciclo a tentativa está
    {
        AguardandoProva, // aberta e o aluno ainda não fez a prova no Moodle
        ProvaEmCorrecao, // aberta, a prova existe no Moodle, mas ainda sem nota (em andamento ou não corrigida)
        AguardandoAprovacao, // aberta, nota >= 7: o administrador pode aprovar
        AguardandoReprovacao, // aberta, nota < 7: o administrador pode reprovar e abrir uma nova
        Aprovada, // fechada pelo administrador com nota >= 7
        Reprovada, // fechada pelo administrador com nota < 7
        ReprovadaPorInatividade // fechada pelo administrador sem prova (o aluno não fez a prova)
    } // fim do enum
} // fim do namespace
