// REVISÃO: arquivo novo. Comportamentos compartilhados do design system, usados por todas as páginas:
//  1) tooltip em qualquer elemento com data-tooltip;  2) toast (notificação temporária);
//  3) botão de formulário em estado "carregando";     4) botão de fechar dos avisos (alert).
(function () { // função que roda na hora e isola as variáveis, para não vazarem para o resto do site
    'use strict'; // modo estrito: o JavaScript avisa de erros bobos em vez de ignorá-los

    var html = document.documentElement; // a tag <html>, onde fica a classe sidebar-collapsed

    // ===== 1) Tooltip =====
    // Um único <div class="tooltip"> é criado na primeira vez e reaproveitado. Fica no <body> (position: fixed), então nenhum
    // contêiner com overflow (como a sidebar) consegue cortá-lo.
    var tooltip = null; // o balão (criado sob demanda)

    function mostrarTooltip(alvo) { // alvo = elemento com data-tooltip
        if (alvo.dataset.tooltipIf === 'sidebar-collapsed' && !html.classList.contains('sidebar-collapsed')) return; // este tooltip só vale com a sidebar recolhida (expandida, o texto já aparece)
        if (!tooltip) { // primeira vez
            tooltip = document.createElement('div'); // cria o balão
            tooltip.className = 'tooltip'; // visual definido em components.css
            tooltip.setAttribute('role', 'tooltip'); // papel semântico
            tooltip.hidden = true; // começa escondido
            document.body.append(tooltip); // coloca no <body>
        } // fim do if
        tooltip.textContent = alvo.dataset.tooltip; // texto do balão (textContent: nunca interpreta HTML)
        tooltip.hidden = false; // mostra para poder medir o tamanho
        var r = alvo.getBoundingClientRect(); // posição do elemento na tela
        var t = tooltip.getBoundingClientRect(); // tamanho do balão
        var lado = alvo.dataset.tooltipPlacement === 'right'; // à direita do elemento (sidebar) ou acima (padrão)
        var x = lado ? r.right + 8 : r.left + (r.width - t.width) / 2; // à direita com 8px de folga, ou centralizado acima
        var y = lado ? r.top + (r.height - t.height) / 2 : r.top - t.height - 6; // centralizado na vertical, ou 6px acima
        tooltip.style.left = Math.max(8, Math.min(x, window.innerWidth - t.width - 8)) + 'px'; // sem sair da tela na horizontal
        tooltip.style.top = Math.max(8, y) + 'px'; // sem sair da tela no topo
    } // fim do mostrarTooltip

    function esconderTooltip() { // some com o balão
        if (tooltip) tooltip.hidden = true; // só esconde (não remove) para reaproveitar
    } // fim do esconderTooltip

    document.addEventListener('mouseover', function (e) { // mouse entrou em algo
        var alvo = e.target.closest('[data-tooltip]'); // dentro de um elemento com tooltip?
        if (alvo) mostrarTooltip(alvo); // mostra
    }); // fim do mouseover
    document.addEventListener('mouseout', function (e) { // mouse saiu de algo
        if (e.target.closest('[data-tooltip]')) esconderTooltip(); // esconde
    }); // fim do mouseout
    document.addEventListener('focusin', function (e) { // teclado: o foco chegou em algo
        var alvo = e.target.closest('[data-tooltip]'); // elemento com tooltip?
        if (alvo) mostrarTooltip(alvo); // mostra também para quem navega pelo teclado
    }); // fim do focusin
    document.addEventListener('focusout', esconderTooltip); // o foco saiu: esconde
    document.addEventListener('keydown', function (e) { // Esc fecha o tooltip (padrão de acessibilidade)
        if (e.key === 'Escape') esconderTooltip(); // esconde
    }); // fim do keydown
    document.addEventListener('click', esconderTooltip, true); // clicar também esconde (a sidebar, por exemplo, muda de largura)

    // ===== 2) Toast =====
    var regiao = document.getElementById('toast-region'); // contêiner fixo no canto da tela (_Layout.cshtml); tem aria-live="polite"

    function toast(mensagem, tipo) { // tipo: 'success' (padrão) ou 'danger'
        if (!regiao) return; // layout sem a região: nada a fazer
        var item = document.createElement('div'); // o toast
        item.className = 'toast'; // visual definido em components.css
        item.setAttribute('role', tipo === 'danger' ? 'alert' : 'status'); // erro é anunciado na hora; sucesso, com educação
        var icone = document.createElement('i'); // ícone do tipo
        icone.className = tipo === 'danger' ? 'bi bi-exclamation-circle text-danger mt-0.5' : 'bi bi-check-circle text-success mt-0.5'; // cor semântica só no ícone, o texto fica neutro
        icone.setAttribute('aria-hidden', 'true'); // decorativo
        var texto = document.createElement('p'); // a mensagem
        texto.className = 'flex-1'; // ocupa o espaço que sobra
        texto.textContent = mensagem; // textContent: nunca interpreta HTML
        var fechar = document.createElement('button'); // botão de fechar
        fechar.type = 'button'; // não envia formulário
        fechar.className = 'alert-close'; // mesmo botão dos avisos
        fechar.setAttribute('aria-label', 'Fechar notificação'); // nome para leitor de tela
        fechar.innerHTML = '<i class="bi bi-x-lg" aria-hidden="true"></i>'; // ícone fixo (sem dados do usuário), então innerHTML é seguro
        item.append(icone, texto, fechar); // monta o toast
        regiao.append(item); // mostra

        function remover() { // tira o toast com uma transição curta
            item.dataset.saindo = 'true'; // dispara o fade-out do CSS
            setTimeout(function () { item.remove(); }, 200); // remove depois da transição
        } // fim do remover
        fechar.addEventListener('click', remover); // fechar manualmente
        if (tipo !== 'danger') setTimeout(remover, 6000); // sucesso some sozinho em 6s; erro fica até ser fechado
    } // fim do toast

    window.PSGN = window.PSGN || {}; // espaço de nomes único do sistema
    window.PSGN.toast = toast; // outras páginas podem chamar PSGN.toast('texto', 'danger')

    document.querySelectorAll('template[data-toast]').forEach(function (modelo) { // o servidor deixa a mensagem num <template data-toast="success">
        toast(modelo.content.textContent.trim(), modelo.dataset.toast); // vira toast ao carregar a página
    }); // fim do forEach

    // ===== 3) Botão de formulário "carregando" =====
    // Uso: <form data-loading> ... <button type="submit" class="btn ..." data-loading-text="Sincronizando…">
    document.addEventListener('submit', function (e) { // qualquer envio de formulário
        var form = e.target; // formulário enviado
        if (!form.hasAttribute('data-loading')) return; // só os que pediram o estado de carregamento
        var botao = form.querySelector('[type="submit"]'); // botão de enviar
        if (!botao) return; // sem botão: nada a fazer
        if (botao.getAttribute('aria-busy') === 'true') { e.preventDefault(); return; } // já está enviando: bloqueia o segundo clique
        botao.setAttribute('aria-busy', 'true'); // o CSS mostra o spinner e bloqueia cliques
        var rotulo = botao.querySelector('[data-loading-label]'); // trecho de texto que muda
        if (rotulo && botao.dataset.loadingText) { // se o botão define um texto de carregamento
            botao.dataset.textoOriginal = rotulo.textContent; // guarda o original (para o botão "voltar" do navegador)
            rotulo.textContent = botao.dataset.loadingText; // ex.: "Sincronizando…"
        } // fim do if
        var icone = botao.querySelector('i'); // o spinner substitui o ícone
        if (icone) icone.hidden = true; // esconde o ícone enquanto carrega
    }); // fim do submit

    window.addEventListener('pageshow', function () { // voltar pelo navegador reaproveita a página; desfaz o estado de carregamento
        document.querySelectorAll('[aria-busy="true"][data-loading-text]').forEach(function (botao) { // botões que ficaram carregando
            botao.removeAttribute('aria-busy'); // volta ao normal
            var rotulo = botao.querySelector('[data-loading-label]'); // trecho de texto
            if (rotulo && botao.dataset.textoOriginal) rotulo.textContent = botao.dataset.textoOriginal; // restaura o texto
            var icone = botao.querySelector('i'); // ícone
            if (icone) icone.hidden = false; // mostra de novo
        }); // fim do forEach
    }); // fim do pageshow

    // ===== 4) Fechar aviso (alert) =====
    document.addEventListener('click', function (e) { // clique em qualquer lugar
        var botao = e.target.closest('[data-dismiss="alert"]'); // foi no X de um aviso?
        if (!botao) return; // não
        var aviso = botao.closest('.alert'); // o aviso inteiro
        if (aviso) aviso.remove(); // remove da página
    }); // fim do click
})(); // fim da função que roda na hora
