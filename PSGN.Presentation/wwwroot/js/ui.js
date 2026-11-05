// Comportamentos do design system, carregados em todas as páginas pelo _Layout:
// tooltip (data-tooltip), toast, botão "carregando" (data-loading), fechar aviso e confirmação de ações (data-confirm).
(function () {
    'use strict';

    var html = document.documentElement;

    // ===== Tooltip: um único balão no <body> (position: fixed), para nenhum overflow cortá-lo. Usado pela sidebar recolhida =====
    var tooltip = null;

    function mostrarTooltip(alvo) {
        if (alvo.dataset.tooltipIf === 'sidebar-collapsed' && !html.classList.contains('sidebar-collapsed')) return;
        if (!tooltip) {
            tooltip = document.createElement('div');
            tooltip.className = 'tooltip';
            tooltip.setAttribute('role', 'tooltip');
            tooltip.hidden = true;
            document.body.append(tooltip);
        }
        tooltip.textContent = alvo.dataset.tooltip;
        tooltip.hidden = false;
        var r = alvo.getBoundingClientRect();
        var t = tooltip.getBoundingClientRect();
        var lado = alvo.dataset.tooltipPlacement === 'right';
        var x = lado ? r.right + 8 : r.left + (r.width - t.width) / 2;
        var y = lado ? r.top + (r.height - t.height) / 2 : r.top - t.height - 6;
        tooltip.style.left = Math.max(8, Math.min(x, window.innerWidth - t.width - 8)) + 'px';
        tooltip.style.top = Math.max(8, y) + 'px';
    }

    function esconderTooltip() {
        if (tooltip) tooltip.hidden = true;
    }

    document.addEventListener('mouseover', function (e) {
        var alvo = e.target.closest('[data-tooltip]');
        if (alvo) mostrarTooltip(alvo);
    });
    document.addEventListener('mouseout', function (e) {
        if (e.target.closest('[data-tooltip]')) esconderTooltip();
    });
    document.addEventListener('focusin', function (e) {
        var alvo = e.target.closest('[data-tooltip]');
        if (alvo) mostrarTooltip(alvo);
    });
    document.addEventListener('focusout', esconderTooltip);
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') esconderTooltip();
    });
    document.addEventListener('click', esconderTooltip, true);

    // ===== Toast: notificação temporária. Usado pelos <template data-toast> das views e por PSGN.toast() (dashboard.js, alunos.js) =====
    var regiao = document.getElementById('toast-region');

    function toast(mensagem, tipo) {
        if (!regiao) return;
        var item = document.createElement('div');
        item.className = 'toast';
        item.setAttribute('role', tipo === 'danger' ? 'alert' : 'status');
        var icone = document.createElement('i');
        icone.className = tipo === 'danger' ? 'bi bi-exclamation-circle text-danger mt-0.5' : 'bi bi-check-circle text-success mt-0.5';
        icone.setAttribute('aria-hidden', 'true');
        var texto = document.createElement('p');
        texto.className = 'flex-1';
        texto.textContent = mensagem;
        var fechar = document.createElement('button');
        fechar.type = 'button';
        fechar.className = 'alert-close';
        fechar.setAttribute('aria-label', 'Fechar notificação');
        fechar.innerHTML = '<i class="bi bi-x-lg" aria-hidden="true"></i>';
        item.append(icone, texto, fechar);
        regiao.append(item);

        function remover() {
            item.dataset.saindo = 'true';
            setTimeout(function () { item.remove(); }, 200);
        }
        fechar.addEventListener('click', remover);
        if (tipo !== 'danger') setTimeout(remover, 6000);
    }

    window.PSGN = window.PSGN || {};
    window.PSGN.toast = toast;

    document.querySelectorAll('template[data-toast]').forEach(function (modelo) {
        toast(modelo.content.textContent.trim(), modelo.dataset.toast);
    });

    // ===== Botão "carregando": <form data-loading> + <button data-loading-text="…">. Usado na sincronização e nas ações de tentativa =====
    document.addEventListener('submit', function (e) {
        var form = e.target;
        if (!form.hasAttribute('data-loading')) return;
        var botao = form.querySelector('[type="submit"]');
        if (!botao) return;
        if (botao.getAttribute('aria-busy') === 'true') { e.preventDefault(); return; }
        botao.setAttribute('aria-busy', 'true');
        var rotulo = botao.querySelector('[data-loading-label]');
        if (rotulo && botao.dataset.loadingText) {
            botao.dataset.textoOriginal = rotulo.textContent;
            rotulo.textContent = botao.dataset.loadingText;
        }
        var icone = botao.querySelector('i');
        if (icone) icone.hidden = true;
    });

    window.addEventListener('pageshow', function () {
        document.querySelectorAll('[aria-busy="true"][data-loading-text]').forEach(function (botao) {
            botao.removeAttribute('aria-busy');
            var rotulo = botao.querySelector('[data-loading-label]');
            if (rotulo && botao.dataset.textoOriginal) rotulo.textContent = botao.dataset.textoOriginal;
            var icone = botao.querySelector('i');
            if (icone) icone.hidden = false;
        });
    });

    // ===== Fechar aviso (_Alert) =====
    document.addEventListener('click', function (e) {
        var botao = e.target.closest('[data-dismiss="alert"]');
        if (!botao) return;
        var aviso = botao.closest('.alert');
        if (aviso) aviso.remove();
    });

    // ===== Confirmação: <form data-confirm="…" data-confirm-titulo data-confirm-acao data-confirm-perigo> abre o diálogo do _Layout.
    // Ouve o submit na fase de captura, antes do estado "carregando", para poder barrar o envio. Usado nas ações de tentativa =====
    var dialogo = document.getElementById('dialogo-confirmar');
    var formPendente = null;
    var submissorPendente = null;

    document.addEventListener('submit', function (e) {
        var form = e.target;
        if (!dialogo || !form.hasAttribute('data-confirm')) return;
        if (form.dataset.confirmado === 'true') { delete form.dataset.confirmado; return; }
        e.preventDefault();
        e.stopImmediatePropagation();
        formPendente = form;
        submissorPendente = e.submitter || null;
        document.getElementById('dialogo-titulo').textContent = form.dataset.confirmTitulo || 'Confirmar ação';
        document.getElementById('dialogo-texto').textContent = form.dataset.confirm;
        var confirmar = dialogo.querySelector('[data-dialogo="confirmar"]');
        confirmar.textContent = form.dataset.confirmAcao || 'Confirmar';
        confirmar.className = 'btn ' + (form.hasAttribute('data-confirm-perigo') ? 'btn-danger' : 'btn-primary');
        dialogo.showModal();
    }, true);

    if (dialogo) {
        dialogo.addEventListener('click', function (e) {
            var botao = e.target.closest('[data-dialogo]');
            if (!botao) return;
            dialogo.close();
            if (botao.dataset.dialogo === 'confirmar' && formPendente) {
                formPendente.dataset.confirmado = 'true';
                if (formPendente.requestSubmit) formPendente.requestSubmit(submissorPendente || undefined);
                else formPendente.submit();
            }
            formPendente = null;
            submissorPendente = null;
        });
        dialogo.addEventListener('close', function () { formPendente = null; submissorPendente = null; });
    }
})();
