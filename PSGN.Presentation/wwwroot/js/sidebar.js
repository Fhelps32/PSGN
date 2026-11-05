// Sidebar (_Sidebar + _Topbar): painel sobreposto abaixo de 1024px e menu recolhível no desktop (estado salvo no localStorage)
(function () {
    'use strict';

    var html = document.documentElement;
    var sidebar = document.getElementById('sidebar');
    var overlay = document.getElementById('sidebarOverlay');
    var btnAbrir = document.getElementById('openSidebar');
    var btnFechar = document.getElementById('closeSidebar');
    var btnRecolher = document.getElementById('toggleSidebar');
    var telaGrande = window.matchMedia('(min-width: 1024px)');

    if (!sidebar) return;

    // Com o painel aberto, o resto da página fica inerte: o Tab não escapa do painel
    var inertes = ['topbar', 'main']
        .map(function (id) { return document.getElementById(id); })
        .filter(Boolean);

    function estaAberto() {
        return sidebar.dataset.open === 'true';
    }

    function abrir() {
        sidebar.dataset.open = 'true';
        overlay.hidden = false;
        if (btnAbrir) btnAbrir.setAttribute('aria-expanded', 'true');
        inertes.forEach(function (el) { el.inert = true; });
        document.body.classList.add('overflow-hidden');
        if (btnFechar) btnFechar.focus();
    }

    function fechar(devolverFoco) {
        sidebar.dataset.open = 'false';
        overlay.hidden = true;
        if (btnAbrir) btnAbrir.setAttribute('aria-expanded', 'false');
        inertes.forEach(function (el) { el.inert = false; });
        document.body.classList.remove('overflow-hidden');
        if (devolverFoco && btnAbrir) btnAbrir.focus();
    }

    if (btnAbrir) btnAbrir.addEventListener('click', abrir);
    if (btnFechar) btnFechar.addEventListener('click', function () { fechar(true); });
    overlay.addEventListener('click', function () { fechar(true); });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && estaAberto()) fechar(true);
    });

    sidebar.addEventListener('click', function (e) {
        if (e.target.closest('a') && estaAberto()) fechar(false);
    });

    // Mudou de modo (girou o tablet, redimensionou) com o painel aberto: volta ao estado limpo
    telaGrande.addEventListener('change', function (e) {
        if (e.matches && estaAberto()) fechar(false);
    });

    // ----- Desktop: recolher/expandir -----
    function aplicarRecolhida(recolhida) {
        html.classList.toggle('sidebar-collapsed', recolhida);
        if (!btnRecolher) return;
        btnRecolher.setAttribute('aria-expanded', String(!recolhida));
        var rotulo = recolhida ? 'Expandir menu' : 'Recolher menu';
        btnRecolher.setAttribute('aria-label', rotulo);
        btnRecolher.dataset.tooltip = rotulo;
    }

    aplicarRecolhida(html.classList.contains('sidebar-collapsed'));

    if (btnRecolher) btnRecolher.addEventListener('click', function () {
        var recolhida = !html.classList.contains('sidebar-collapsed');
        aplicarRecolhida(recolhida);
        try { localStorage.setItem('sidebarCollapsed', String(recolhida)); } catch (err) { /* localStorage bloqueado (modo privado): só não lembra da escolha */ }
    });
})();
