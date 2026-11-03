// REVISÃO: arquivo novo, da página Alunos/Index. Faz duas coisas: (1) expande/recolhe a linha com os módulos de cada aluno e
// (2) preenche o select de Módulo quando o Curso muda (a "cascata" Curso -> Módulo, a mesma ideia do filtro de Inscrições do PSG).
(function () { // função que roda na hora e isola as variáveis, para não vazarem para o resto do site
    'use strict'; // modo estrito: o JavaScript avisa de erros bobos em vez de ignorá-los

    // ===== Expandir/recolher os módulos de um aluno =====
    document.addEventListener('click', function (e) { // um único ouvinte para a página toda (event delegation): vale para todos os botões
        var botao = e.target.closest('[data-alunos-toggle]'); // o clique foi num botão de expandir (ou no ícone dentro dele)?
        if (!botao) return; // clicou em outra coisa: nada a fazer

        var linha = document.getElementById(botao.getAttribute('data-alunos-toggle')); // linha escondida ligada ao botão
        var vaiAbrir = linha.hidden; // se está escondida, o clique vai abrir
        linha.hidden = !vaiAbrir; // mostra ou esconde a linha
        botao.setAttribute('aria-expanded', String(vaiAbrir)); // avisa o leitor de tela do novo estado
        var nome = botao.getAttribute('aria-label').replace(/^(Mostrar|Esconder)/, ''); // pega o " módulos de Fulano" do texto atual
        botao.setAttribute('aria-label', (vaiAbrir ? 'Esconder' : 'Mostrar') + nome); // troca o verbo do texto do botão
        botao.querySelector('i').style.transform = vaiAbrir ? 'rotate(90deg)' : ''; // gira a seta para baixo quando aberto
    }); // fim do click

    // ===== Cascata Curso -> Módulo no filtro =====
    var form = document.querySelector('form[data-url-modulos]'); // formulário de filtro (a URL do endpoint vem no atributo data-url-modulos)
    if (!form) return; // página sem o filtro: nada a fazer

    var selectCurso = document.getElementById('CursoId'); // id gerado pelo asp-for="CursoId"
    var selectModulo = document.getElementById('ModuloId'); // id gerado pelo asp-for="ModuloId"
    if (!selectCurso || !selectModulo) return; // sem os dois selects não há cascata

    function opcao(valor, texto) { // cria um <option>
        var o = document.createElement('option'); // elemento novo
        o.value = valor; // o que vai para a URL
        o.textContent = texto; // textContent (e não innerHTML) para o nome do módulo nunca ser interpretado como HTML
        return o; // devolve a opção pronta
    } // fim do opcao

    selectCurso.addEventListener('change', async function () { // quando o curso do filtro muda
        selectModulo.replaceChildren(opcao('', 'Todos')); // zera os módulos e deixa só "Todos"

        if (!selectCurso.value) { // voltou para "Todos os cursos"
            selectModulo.disabled = true; // sem curso não há módulos para escolher (e select desabilitado não vai na URL)
            return; // fim
        } // fim do if

        selectModulo.disabled = true; // trava enquanto carrega, para não escolher uma opção que vai sumir
        var resp = await fetch(form.dataset.urlModulos + '?cursoId=' + encodeURIComponent(selectCurso.value)); // pede os módulos do curso ao AlunosController
        if (!resp.ok) return; // se deu erro no servidor, deixa só "Todos" (e desabilitado)

        var modulos = await resp.json(); // formato: [{ value: 12, text: "Nome do módulo" }, ...]
        modulos.forEach(function (m) { selectModulo.append(opcao(m.value, m.text)); }); // uma opção por módulo
        selectModulo.disabled = false; // libera o select
    }); // fim do change
})(); // fim da função que roda na hora
