// Página de Alunos: mantém o aluno selecionado visível na lista, rola até o detalhe em telas estreitas,
// busca instantânea dos módulos e cascata Curso -> Módulo no filtro.
(function () {
    'use strict';

    // ===== Aluno selecionado visível (rola só a lista, não a página) =====
    var lista = document.getElementById('listaAlunos');
    var selecionado = lista && lista.querySelector('[aria-current="true"]');
    if (lista && selecionado && lista.scrollHeight > lista.clientHeight) {
        var item = selecionado.parentElement;
        var topo = item.offsetTop;
        if (topo + item.offsetHeight > lista.clientHeight)
            lista.scrollTop = topo - (lista.clientHeight - item.offsetHeight) / 2;
    }

    // ===== Abaixo de 1280px o detalhe fica embaixo da lista: rola até ele ao escolher um aluno =====
    var detalhe = document.getElementById('detalhe-aluno');
    var escolheu = /[?&]alunoid=/i.test(window.location.search);
    if (detalhe && escolheu && !window.matchMedia('(min-width: 1280px)').matches)
        detalhe.scrollIntoView();

    // ===== Busca instantânea dos módulos (card Módulos) =====
    var campoModulo = document.getElementById('buscaModulo');
    var listaModulos = document.getElementById('listaModulos');
    if (campoModulo && listaModulos) {
        var modulos = Array.prototype.slice.call(listaModulos.querySelectorAll('.modulo-item'));
        var aviso = document.getElementById('semResultadoModulo');
        campoModulo.addEventListener('input', function () {
            var termo = campoModulo.value.trim().toLowerCase();
            var visiveis = 0;
            modulos.forEach(function (m) {
                var bate = !termo || m.dataset.nome.toLowerCase().indexOf(termo) !== -1;
                m.hidden = !bate;
                if (bate) visiveis++;
            });
            if (aviso) aviso.classList.toggle('hidden', visiveis > 0);
        });
    }

    // ===== Cascata Curso -> Módulo (endpoint AlunosController.GetModulosPorCurso) =====
    var form = document.querySelector('form[data-url-modulos]');
    if (!form) return;

    var selectCurso = document.getElementById('CursoId');
    var selectModulo = document.getElementById('ModuloId');
    if (!selectCurso || !selectModulo) return;

    function opcao(valor, texto) {
        var o = document.createElement('option');
        o.value = valor;
        o.textContent = texto;
        return o;
    }

    selectCurso.addEventListener('change', async function () {
        selectModulo.replaceChildren(opcao('', 'Todos os módulos'));

        if (!selectCurso.value) {
            selectModulo.disabled = true;
            return;
        }

        selectModulo.disabled = true;
        var resp = await fetch(form.dataset.urlModulos + '?cursoId=' + encodeURIComponent(selectCurso.value));
        if (!resp.ok) { if (window.PSGN) PSGN.toast('Não foi possível carregar os módulos do curso.', 'danger'); return; }

        var modulos = await resp.json();
        modulos.forEach(function (m) { selectModulo.append(opcao(m.value, m.text)); });
        selectModulo.disabled = false;
    });
})();
