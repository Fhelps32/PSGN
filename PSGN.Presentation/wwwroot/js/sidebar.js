// REVISÃO: arquivo novo. Antes o JavaScript da sidebar ficava inline no _Sidebar.cshtml e só sabia recolher/expandir.
// Agora ele controla os DOIS modos da sidebar (ver Styles/layout.css): painel sobreposto no celular/tablet e sidebar recolhível no desktop.
(function () { // função que roda na hora e isola as variáveis, para não vazarem para o resto do site
    'use strict'; // modo estrito: o JavaScript avisa de erros bobos em vez de ignorá-los

    var html = document.documentElement; // a tag <html>, onde fica a classe sidebar-collapsed
    var sidebar = document.getElementById('sidebar'); // o painel lateral
    var overlay = document.getElementById('sidebarOverlay'); // o fundo escuro atrás do painel aberto
    var btnAbrir = document.getElementById('openSidebar'); // botão da barra superior (só aparece abaixo de 1024px)
    var btnFechar = document.getElementById('closeSidebar'); // botão X dentro do painel (só aparece abaixo de 1024px)
    var btnRecolher = document.getElementById('toggleSidebar'); // botão de recolher/expandir (só aparece a partir de 1024px)
    var telaGrande = window.matchMedia('(min-width: 1024px)'); // o mesmo ponto de quebra do layout.css ("lg" do Tailwind)

    if (!sidebar) return; // página sem sidebar (ex.: uma view que use outro layout): não há nada para controlar

    // Elementos que ficam "inertes" (sem clique, sem foco, invisíveis ao leitor de tela) enquanto o painel está aberto.
    // Com o resto da página inerte o Tab não escapa do painel, que é o que um diálogo modal precisa fazer.
    var inertes = ['topbar', 'main'] // ids dos elementos
        .map(function (id) { return document.getElementById(id); }) // procura cada um na página
        .filter(Boolean); // descarta os que não existirem

    function estaAberto() { // o painel está aberto?
        return sidebar.dataset.open === 'true'; // o estado mora no atributo data-open (o layout.css reage a ele)
    } // fim do estaAberto

    function abrir() { // abre o painel sobreposto (celular/tablet)
        sidebar.dataset.open = 'true'; // o layout.css traz o painel para dentro da tela
        overlay.hidden = false; // mostra o fundo escuro
        if (btnAbrir) btnAbrir.setAttribute('aria-expanded', 'true'); // avisa o leitor de tela que o menu abriu
        inertes.forEach(function (el) { el.inert = true; }); // trava o resto da página
        document.body.classList.add('overflow-hidden'); // impede a página de rolar por trás do painel
        if (btnFechar) btnFechar.focus(); // leva o foco para dentro do painel, senão o teclado continuaria "atrás" dele
    } // fim do abrir

    function fechar(devolverFoco) { // fecha o painel sobreposto
        sidebar.dataset.open = 'false'; // o layout.css empurra o painel para fora da tela
        overlay.hidden = true; // esconde o fundo escuro
        if (btnAbrir) btnAbrir.setAttribute('aria-expanded', 'false'); // avisa o leitor de tela que o menu fechou
        inertes.forEach(function (el) { el.inert = false; }); // libera a página de novo
        document.body.classList.remove('overflow-hidden'); // a página volta a rolar
        if (devolverFoco && btnAbrir) btnAbrir.focus(); // devolve o foco ao botão que abriu, para o usuário não "se perder" na página
    } // fim do fechar

    if (btnAbrir) btnAbrir.addEventListener('click', abrir); // botão da barra superior abre o painel
    if (btnFechar) btnFechar.addEventListener('click', function () { fechar(true); }); // X fecha e devolve o foco
    overlay.addEventListener('click', function () { fechar(true); }); // clicar no fundo escuro fecha

    document.addEventListener('keydown', function (e) { // teclado
        if (e.key === 'Escape' && estaAberto()) fechar(true); // Esc fecha o painel (padrão esperado de qualquer menu sobreposto)
    }); // fim do keydown

    sidebar.addEventListener('click', function (e) { // cliques dentro da sidebar
        if (e.target.closest('a') && estaAberto()) fechar(false); // clicou num link: fecha (a página vai navegar, então não precisa devolver o foco)
    }); // fim do click na sidebar

    // Se a janela passar de um modo para o outro com o painel aberto (girar o tablet, redimensionar), volta ao estado limpo
    telaGrande.addEventListener('change', function (e) { // dispara quando cruza os 1024px
        if (e.matches && estaAberto()) fechar(false); // ficou grande: o painel sobreposto não faz mais sentido
    }); // fim do change

    // ----- Desktop: recolher/expandir -----
    function aplicarRecolhida(recolhida) { // atualiza a tela e os atributos de acessibilidade do botão
        html.classList.toggle('sidebar-collapsed', recolhida); // a classe no <html> é o que o layout.css usa para estreitar a sidebar
        if (!btnRecolher) return; // sem botão, nada mais a atualizar
        btnRecolher.setAttribute('aria-expanded', String(!recolhida)); // expandida = true
        var rotulo = recolhida ? 'Expandir menu' : 'Recolher menu'; // texto da ação que o botão faz AGORA
        btnRecolher.setAttribute('aria-label', rotulo); // nome lido pelo leitor de tela (o botão só tem ícone)
        btnRecolher.dataset.tooltip = rotulo; // REVISÃO: o tooltip do ui.js acompanha a ação atual. O texto visível que ficava no rodapé saiu: o botão agora fica no cabeçalho, só com ícone
    } // fim do aplicarRecolhida

    aplicarRecolhida(html.classList.contains('sidebar-collapsed')); // sincroniza os atributos com a classe que o _Layout já aplicou antes de desenhar

    if (btnRecolher) btnRecolher.addEventListener('click', function () { // clique no botão do desktop
        var recolhida = !html.classList.contains('sidebar-collapsed'); // inverte o estado atual
        aplicarRecolhida(recolhida); // aplica
        try { localStorage.setItem('sidebarCollapsed', String(recolhida)); } catch (err) { /* localStorage bloqueado (modo privado): só não lembra da escolha */ } // guarda para a próxima página abrir igual
    }); // fim do click do recolher
})(); // fim da função que roda na hora
