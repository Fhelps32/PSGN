// REVISÃO: alunos.js da página mestre-detalhe (distribuição da página de Alunos do PSG). Faz três coisas:
// (1) mantém o aluno selecionado visível na lista (selecionar recarrega a página e a lista voltaria ao topo), como na página de Alunos do PSG;
// (2) busca instantânea dos módulos do aluno selecionado (card "Módulos");
// (3) preenche o select de Módulo quando o Curso muda (a "cascata" Curso -> Módulo).
(function () { // função que roda na hora e isola as variáveis, para não vazarem para o resto do site
    'use strict'; // modo estrito: o JavaScript avisa de erros bobos em vez de ignorá-los

    // ===== (1) Aluno selecionado visível =====
    var lista = document.getElementById('listaAlunos'); // a lista com rolagem própria (o <ul> é "relative", então offsetTop é a posição dentro dele)
    var selecionado = lista && lista.querySelector('[aria-current="true"]'); // o link do aluno mostrado à direita
    if (lista && selecionado && lista.scrollHeight > lista.clientHeight) { // só quando a lista tem rolagem própria
        var item = selecionado.parentElement; // o <li> que envolve o link
        var topo = item.offsetTop; // posição do item dentro da lista
        if (topo + item.offsetHeight > lista.clientHeight) // o item está abaixo da parte visível?
            lista.scrollTop = topo - (lista.clientHeight - item.offsetHeight) / 2; // rola SÓ a lista (não a página) até centralizar o item
    } // fim do if

    // ===== (1b) Em telas estreitas o detalhe fica ABAIXO da lista: ao escolher um aluno, rola até ele =====
    // (no desktop o detalhe já está ao lado da lista, então a página não precisa rolar)
    var detalhe = document.getElementById('detalhe-aluno'); // o painel do aluno selecionado
    var escolheu = /[?&]alunoid=/i.test(window.location.search); // a URL traz o aluno escolhido (o nome do parâmetro vem em qualquer caixa)
    if (detalhe && escolheu && !window.matchMedia('(min-width: 1280px)').matches) // só abaixo de 1280px (o breakpoint xl do layout)
        detalhe.scrollIntoView(); // o scroll-mt-20 do painel deixa folga para a barra superior

    // ===== (2) Busca instantânea dos módulos do aluno =====
    var campoModulo = document.getElementById('buscaModulo'); // campo "Pesquisar" do card Módulos
    var listaModulos = document.getElementById('listaModulos'); // a lista de módulos (já vem toda renderizada)
    if (campoModulo && listaModulos) { // só existe quando o aluno selecionado tem módulos
        var modulos = Array.prototype.slice.call(listaModulos.querySelectorAll('.modulo-item')); // um item por módulo
        var aviso = document.getElementById('semResultadoModulo'); // "Nenhum módulo encontrado"
        campoModulo.addEventListener('input', function () { // a cada letra digitada
            var termo = campoModulo.value.trim().toLowerCase(); // texto buscado, sem diferenciar maiúsculas
            var visiveis = 0; // quantos módulos sobraram
            modulos.forEach(function (m) { // cada módulo
                var bate = !termo || m.dataset.nome.toLowerCase().indexOf(termo) !== -1; // o nome contém o texto?
                m.hidden = !bate; // esconde (atributo hidden) quem não bate
                if (bate) visiveis++; // conta quem ficou
            }); // fim do forEach
            if (aviso) aviso.classList.toggle('hidden', visiveis > 0); // mostra o aviso só quando não sobrou nenhum
        }); // fim do input
    } // fim do if

    // ===== (3) Cascata Curso -> Módulo no filtro =====
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
