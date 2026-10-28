namespace PSGN.Domain.Enum // mesma pasta "Enum" que o PSG usa para o EnumStatus
{
    // REVISÃO: enum novo. O PSG guardava o status no banco; o PSGN ainda não tem essa coluna, então a situação é CALCULADA
    // a partir do que já existe (Status do AlunoModulo e notas das tentativas), pelo método AlunoModulo.ObterSituacao
    public enum EnumSituacaoInscricao // situação de um aluno em um módulo
    {
        EmAndamento, // inscrito, sem tentativa aprovada nem reprovada ainda
        Aprovado, // tem pelo menos uma tentativa com nota maior ou igual à nota mínima
        Reprovado, // terminou tentativa(s) corrigida(s), mas nenhuma atingiu a nota mínima
        Cancelado // a inscrição (AlunoModulo) ou o aluno está com Status = false
    } // fim do enum
} // fim do namespace
