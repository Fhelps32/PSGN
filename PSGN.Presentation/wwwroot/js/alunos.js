// REVISÃO: alunos.js da página mestre-detalhe. Faz duas coisas:
// (1) mantém o aluno selecionado visível na lista (selecionar recarrega a página e a lista voltaria ao topo), como na página de Alunos do PSG;
// (2) preenche o select de Módulo quando o Curso muda (a "cascata" Curso -> Módulo).
// O código que expandia as linhas da tabela antiga saiu: o histórico agora fica no painel da direita.
(function () { // função que roda na hora e isola as variáveis, para não vazarem para o resto do site
    'use strict'; // modo estrito: o JavaScript avisa de erros bobos em vez de ignorá-los

    // ===== (1) Aluno selecionado visível =====
    var lista = document.getElementById('listaAlunos'); // a lista com rolagem própria (desktop)
    var selecionado = lista && lista.querySelector('[aria-current="true"]'); // o item do aluno mostrado à direita
    if (lista && selecionado && lista.scrollHeight > lista.clientHeight) { // só quando a lista tem rolagem própria
        var topo = selecionado.offsetTop - lista.offsetTop; // posição do item dentro da lista
        if (topo + selecionado.offsetHeight > lista.clientHeight) // o item está abaixo da parte visível?
            lista.scrollTop = topo - (lista.clientHeight - selecionado.offsetHeight) / 2; // rola SÓ a lista (não a página) até centralizar o item
    } // fim do if

    // ===== (2) Cascata Curso -> Módulo no filtro =====
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
        selectModulo.replaceChildren(opcao('', 'Todos os módulos')); // zera os módulos e deixa só "Todos os módulos"

        if (!selectCurso.value) { // voltou para "Todos os cursos"
            selectModulo.disabled = true; // sem curso não há módulos para escolher (e select desabilitado não vai na URL)
            return; // fim
        } // fim do if

        selectModulo.disabled = true; // trava enquanto carrega, para não escolher uma opção que vai sumir
        var resp = await fetch(form.dataset.urlModulos + '?cursoId=' + encodeURIComponent(selectCurso.value)); // pede os módulos do curso ao AlunosController
        if (!resp.ok) { if (window.PSGN) PSGN.toast('Não foi possível carregar os módulos do curso.', 'danger'); return; } // erro visível; o select continua só com "Todos os módulos"

        var modulos = await resp.json(); // formato: [{ value: 12, text: "Nome do módulo" }, ...]
        modulos.forEach(function (m) { selectModulo.append(opcao(m.value, m.text)); }); // uma opção por módulo
        selectModulo.disabled = false; // libera o select
    }); // fim do change
})(); // fim da função que roda na hora
