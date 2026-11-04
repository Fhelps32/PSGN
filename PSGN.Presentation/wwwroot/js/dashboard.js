// REVISÃO: dashboard.js reescrito para o redesign (e atualizado para os papéis de cor --color-* da nova identidade cromática). Mudanças: (1) todas as cores e a fonte vêm dos tokens do CSS (Styles/tokens.css), sem hex no JavaScript;
// (2) paleta categórica da pizza validada (6 cores, ordem fixa, distinguível por daltônicos); (3) legenda com quantidade E porcentagem; (4) estado de carregamento nos cards
// e erro visível (toast) quando o AJAX falha, em vez de falhar em silêncio. Os dados e as URLs chegam por <script id="dashboard-data" type="application/json">.
(function () { // função que roda na hora e isola as variáveis, para não vazarem para o resto do site
    'use strict'; // modo estrito: o JavaScript avisa de erros bobos em vez de ignorá-los

    var bloco = document.getElementById('dashboard-data'); // bloco JSON que a view escreveu
    if (!bloco || typeof Chart === 'undefined') return; // sem dados ou sem o Chart.js carregado não há o que desenhar
    var dados = JSON.parse(bloco.textContent); // { lineLabels, lineData, pieLabels, pieData, urlLinha, urlPizza }

    // ===== Tokens do design system =====
    var estilo = getComputedStyle(document.documentElement); // estilos calculados do <html>, onde os tokens (:root) estão
    function token(nome) { return estilo.getPropertyValue(nome).trim(); } // lê um token, ex.: token('--color-text-muted')
    function rgb(nome, alfa) { // converte um token de cor ("37 99 235") para uma cor que o canvas entende
        var c = token(nome).split(/\s+/); // separa R, G e B
        return alfa === undefined ? 'rgb(' + c.join(',') + ')' : 'rgba(' + c.join(',') + ',' + alfa + ')'; // com ou sem transparência
    } // fim do rgb

    var coresPizza = [1, 2, 3, 4, 5, 6].map(function (n) { return token('--color-chart-' + n); }); // paleta categórica (--color-chart-1..6); a legenda HTML usa as mesmas
    var corLinha = token('--color-chart-1'); // a série única do gráfico em linha usa o indigo (série 1 da paleta)

    Chart.defaults.font.family = token('--font-sans'); // Inter também dentro dos gráficos
    Chart.defaults.font.size = 12; // 12px: mesmo tamanho dos rótulos do sistema
    Chart.defaults.color = rgb('--color-text-muted'); // cor dos textos dos eixos (contraste mínimo de 4,5:1)

    var tooltipPadrao = { // tooltip no estilo do sistema: escuro, cantos de 6px, sem caixinha de cor
        backgroundColor: rgb('--color-text-primary'), // mesmo fundo do tooltip HTML (.tooltip)
        padding: 8, // respiro interno
        cornerRadius: 6, // mesmo raio dos controles (--radius-control)
        displayColors: false, // sem quadradinho de cor: o título já diz de quem é o valor
        titleFont: { weight: '500' }, // título em peso médio
        bodyFont: { weight: '400' } // corpo normal
    }; // fim do tooltipPadrao

    // Texto alternativo do gráfico, lido por leitores de tela (o <canvas> sozinho é uma imagem muda para eles)
    function descrever(titulo, rotulos, valores) { // titulo = frase de abertura; rotulos e valores = os pontos do gráfico
        var itens = rotulos.map(function (rotulo, i) { return rotulo + ': ' + valores[i]; }); // "01/2026: 3"
        return titulo + (itens.length ? '. ' + itens.join('; ') : '. Sem dados.'); // junta tudo numa frase só
    } // fim do descrever

    // Carregamento: o véu com spinner (.is-loading) cobre o card enquanto a requisição não volta
    function carregando(card, ligado) { // card = <section> do gráfico
        card.classList.toggle('is-loading', ligado); // liga/desliga o véu
        card.setAttribute('aria-busy', String(ligado)); // avisa o leitor de tela
    } // fim do carregando

    function falhou(mensagem) { // erro visível: toast que fica até ser fechado
        if (window.PSGN && PSGN.toast) PSGN.toast(mensagem, 'danger'); // usa o toast do ui.js
    } // fim do falhou

    // ===== Gráfico em linha: alunos cadastrados por mês =====
    var canvasLinha = document.getElementById('lineGraphAlunos'); // <canvas> do gráfico em linha
    canvasLinha.setAttribute('aria-label', descrever('Gráfico de linha: alunos cadastrados por mês', dados.lineLabels, dados.lineData)); // texto alternativo com os valores reais
    new Chart(canvasLinha, { // desenha no <canvas id="lineGraphAlunos">
        type: 'line', // gráfico de linha
        data: { // dados
            labels: dados.lineLabels, // eixo X
            datasets: [{ // uma única série (por isso não há legenda: o título do card já diz o que é)
                label: 'Alunos cadastrados', // nome da série (aparece no tooltip)
                data: dados.lineData, // eixo Y
                borderColor: corLinha, // linha de 2px no azul da marca
                borderWidth: 2, // espessura fina
                backgroundColor: rgb('--color-brand-primary', 0.08), // preenchimento bem leve abaixo da linha
                tension: 0.3, // suaviza a curva
                fill: true, // preenche abaixo da linha
                pointRadius: 3, // pontos pequenos
                pointHoverRadius: 5, // aumentam ao passar o mouse
                pointBackgroundColor: corLinha, // miolo do ponto
                pointBorderColor: rgb('--color-surface-primary'), // anel branco separa o ponto da linha
                pointBorderWidth: 2 // espessura do anel
            }] // fim da série
        }, // fim dos dados
        options: { // opções
            responsive: true, // acompanha o tamanho do container
            maintainAspectRatio: false, // usa a altura do container (h-72) em vez de uma proporção fixa
            interaction: { mode: 'index', intersect: false }, // o tooltip aparece ao passar perto do ponto, não só em cima dele
            plugins: { legend: { display: false }, tooltip: tooltipPadrao }, // sem legenda (série única); tooltip do sistema
            scales: { // eixos discretos
                x: { grid: { display: false }, border: { color: rgb('--color-border-default') } }, // sem grade vertical
                y: { beginAtZero: true, ticks: { precision: 0 }, grid: { color: rgb('--color-border-default') }, border: { display: false } } // começa no 0, só inteiros, grade clara e sem linha do eixo
            } // fim dos eixos
        } // fim das opções
    }); // fim do gráfico em linha

    // ===== Gráfico de pizza: alunos por curso (ou por módulo) =====
    var canvasPizza = document.getElementById('pieGraphCursos'); // <canvas> do gráfico de pizza
    new Chart(canvasPizza, { // desenha no <canvas id="pieGraphCursos">
        type: 'pie', // gráfico de pizza
        data: { // dados
            labels: dados.pieLabels, // nome de cada fatia (aparece no tooltip e na legenda HTML)
            datasets: [{ // uma única série
                data: dados.pieData, // tamanho das fatias
                backgroundColor: coresPizza, // cores validadas; o Chart.js repete em ciclo se houver mais de 6 fatias
                borderColor: rgb('--color-surface-primary'), // 2px da cor do card entre as fatias: separa as cores vizinhas
                borderWidth: 2, // espessura da separação
                hoverOffset: 4 // a fatia sob o mouse salta um pouco
            }] // fim da série
        }, // fim dos dados
        options: { // opções
            responsive: true, // acompanha o tamanho do container
            maintainAspectRatio: false, // usa o tamanho do container (h-48 w-48)
            plugins: { // plugins
                legend: { display: false }, // legenda do Chart.js desligada; quem mostra é a <ul id="pieLegenda">
                tooltip: Object.assign({}, tooltipPadrao, { // tooltip da pizza: mostra o nome da fatia e o valor
                    callbacks: { // personaliza o texto
                        title: function () { return ''; }, // sem título (o nome já vem no corpo)
                        label: function (item) { return item.label + ': ' + item.formattedValue; } // "Curso: 30"
                    } // fim dos callbacks
                }) // fim do tooltip
            } // fim dos plugins
        } // fim das opções
    }); // fim do gráfico de pizza

    // Legenda HTML da pizza: no PSG a legenda era "Curso - Módulo"; no PSGN os nomes são longos. Como o curso já aparece no select,
    // a fatia de módulo mostra só o nome do módulo
    function rotuloFatia(d) { return d.modulo ? d.modulo : d.curso; } // monta a legenda a partir do JSON do AJAX (módulo só existe com curso escolhido)

    // Monta a legenda a partir do que está no gráfico (chamada ao abrir a página e depois de cada AJAX)
    function desenharLegendaPizza() {
        var chart = Chart.getChart('pieGraphCursos'); // gráfico já desenhado
        var lista = document.getElementById('pieLegenda'); // <ul> da legenda
        var valores = chart.data.datasets[0].data; // quantidades
        var total = valores.reduce(function (soma, v) { return soma + v; }, 0); // total, para a porcentagem
        lista.replaceChildren(); // apaga a legenda anterior
        chart.data.labels.forEach(function (rotulo, i) { // um item por fatia
            var item = document.createElement('li'); // linha da legenda
            item.className = 'flex items-start gap-2.5'; // quadradinho, texto e números lado a lado
            var cor = document.createElement('span'); // quadradinho com a cor da fatia
            cor.className = 'mt-1 h-2.5 w-2.5 shrink-0 rounded-sm'; // tamanho e alinhamento com a 1ª linha do texto
            cor.style.backgroundColor = coresPizza[i % coresPizza.length]; // mesma regra de ciclo de cores do Chart.js
            var texto = document.createElement('span'); // nome do curso/módulo
            texto.className = 'min-w-0 flex-1 break-words leading-snug text-ink-soft'; // ocupa o espaço que sobra e quebra linha se for longo
            texto.textContent = rotulo; // textContent (e não innerHTML) para o nome nunca ser interpretado como HTML
            var numeros = document.createElement('span'); // quantidade e porcentagem
            numeros.className = 'shrink-0 text-right tabular-nums text-ink'; // números alinhados
            var pct = total > 0 ? Math.round(valores[i] / total * 100) : 0; // porcentagem arredondada
            numeros.innerHTML = '<span class="font-medium">' + valores[i] + '</span> <span class="text-ink-muted">' + pct + '%</span>'; // só números (nunca texto do usuário), então innerHTML é seguro
            item.append(cor, texto, numeros); // monta a linha
            lista.append(item); // coloca a linha na legenda
        }); // fim do forEach
        canvasPizza.setAttribute('aria-label', descrever('Gráfico de pizza: alunos por curso', chart.data.labels, valores)); // mantém o texto alternativo igual ao que está na tela
    } // fim do desenharLegendaPizza

    desenharLegendaPizza(); // desenha a legenda da primeira renderização

    // ===== AJAX do gráfico em linha =====
    async function atualizarGraficoLinha() { // chamado quando o período ou o curso muda
        var card = document.getElementById('cardLinha'); // card do gráfico, para o véu de carregamento
        var tempo = document.getElementById('selectTempoLinha').value; // quantidade de meses escolhida
        var curso = document.getElementById('selectCursoLinha').value; // id do curso ("" = todos)
        carregando(card, true); // mostra o véu
        try { // a rede pode falhar
            var resp = await fetch(dados.urlLinha + '?tempo=' + encodeURIComponent(tempo) + '&cursoId=' + encodeURIComponent(curso)); // pede os pontos novos ao DashboardController
            if (!resp.ok) throw new Error('HTTP ' + resp.status); // erro do servidor
            var pontos = await resp.json(); // formato: [{ totalAlunos: 10, dataRegistro: "2026-01-01T00:00:00" }, ...]
            var chart = Chart.getChart('lineGraphAlunos'); // pega o gráfico já desenhado
            chart.data.labels = pontos.map(function (d) { return new Date(d.dataRegistro).toLocaleDateString('pt-BR', { month: '2-digit', year: 'numeric' }); }); // novos rótulos MM/yyyy
            chart.data.datasets[0].data = pontos.map(function (d) { return d.totalAlunos; }); // novos valores
            chart.update(); // redesenha com animação
            canvasLinha.setAttribute('aria-label', descrever('Gráfico de linha: alunos cadastrados por mês', chart.data.labels, chart.data.datasets[0].data)); // atualiza o texto alternativo
        } catch (e) { // algo falhou
            falhou('Não foi possível atualizar o histórico de alunos. O gráfico continua com os dados anteriores.'); // erro visível (antes era silencioso)
        } finally { // sempre
            carregando(card, false); // tira o véu
        } // fim do try
    } // fim do atualizarGraficoLinha

    document.getElementById('selectTempoLinha').addEventListener('change', atualizarGraficoLinha); // troca de período
    document.getElementById('selectCursoLinha').addEventListener('change', atualizarGraficoLinha); // troca de curso

    // ===== AJAX do gráfico de pizza =====
    document.getElementById('selectCursoPizza').addEventListener('change', async function () { // quando o curso da pizza muda
        var card = document.getElementById('cardPizza'); // card do gráfico, para o véu de carregamento
        var cursoId = document.getElementById('selectCursoPizza').value; // id do curso ("" = todos)
        carregando(card, true); // mostra o véu
        try { // a rede pode falhar
            var resp = await fetch(dados.urlPizza + '?cursoId=' + encodeURIComponent(cursoId)); // pede as fatias novas ao DashboardController
            if (!resp.ok) throw new Error('HTTP ' + resp.status); // erro do servidor
            var fatias = await resp.json(); // formato: [{ curso: "...", modulo: "..." | null, quantidadeAlunos: 5 }, ...]
            var chart = Chart.getChart('pieGraphCursos'); // pega o gráfico já desenhado
            chart.data.labels = fatias.map(rotuloFatia); // nova legenda
            chart.data.datasets[0].data = fatias.map(function (d) { return d.quantidadeAlunos; }); // novos tamanhos
            chart.update(); // redesenha com animação
            desenharLegendaPizza(); // atualiza a legenda HTML (e o texto alternativo) com as fatias novas
        } catch (e) { // algo falhou
            falhou('Não foi possível atualizar o gráfico de alunos por curso. O gráfico continua com os dados anteriores.'); // erro visível
        } finally { // sempre
            carregando(card, false); // tira o véu
        } // fim do try
    }); // fim do evento da pizza
})(); // fim da função que roda na hora
