// Gráficos do Dashboard (Chart.js): linha (alunos por mês) e pizza (alunos por curso/módulo), com filtros via fetch.
// Cores e fonte vêm dos tokens do CSS; dados e URLs chegam pelo <script id="dashboard-data"> da view.
(function () {
    'use strict';

    var bloco = document.getElementById('dashboard-data');
    if (!bloco || typeof Chart === 'undefined') return;
    var dados = JSON.parse(bloco.textContent);

    // ===== Tokens do design system (lidos do CSS) =====
    var estilo = getComputedStyle(document.documentElement);
    function token(nome) { return estilo.getPropertyValue(nome).trim(); }
    function rgb(nome, alfa) {
        var c = token(nome).split(/\s+/);
        return alfa === undefined ? 'rgb(' + c.join(',') + ')' : 'rgba(' + c.join(',') + ',' + alfa + ')';
    }

    var coresPizza = [1, 2, 3, 4, 5, 6].map(function (n) { return token('--color-chart-' + n); });
    var corLinha = token('--color-chart-1');

    Chart.defaults.font.family = token('--font-sans');
    Chart.defaults.font.size = 12;
    Chart.defaults.color = rgb('--color-text-muted');

    var tooltipPadrao = {
        backgroundColor: rgb('--color-text-primary'),
        padding: 8,
        cornerRadius: 6,
        displayColors: false,
        titleFont: { weight: '500' },
        bodyFont: { weight: '400' }
    };

    // Texto alternativo do <canvas> para leitores de tela
    function descrever(titulo, rotulos, valores) {
        var itens = rotulos.map(function (rotulo, i) { return rotulo + ': ' + valores[i]; });
        return titulo + (itens.length ? '. ' + itens.join('; ') : '. Sem dados.');
    }

    // Véu com spinner (.is-loading) enquanto a requisição não volta
    function carregando(card, ligado) {
        card.classList.toggle('is-loading', ligado);
        card.setAttribute('aria-busy', String(ligado));
    }

    function falhou(mensagem) {
        if (window.PSGN && PSGN.toast) PSGN.toast(mensagem, 'danger');
    }

    // ===== Gráfico em linha: alunos cadastrados por mês =====
    var canvasLinha = document.getElementById('lineGraphAlunos');
    canvasLinha.setAttribute('aria-label', descrever('Gráfico de linha: alunos cadastrados por mês', dados.lineLabels, dados.lineData));
    new Chart(canvasLinha, {
        type: 'line',
        data: {
            labels: dados.lineLabels,
            datasets: [{
                label: 'Alunos cadastrados',
                data: dados.lineData,
                borderColor: corLinha,
                borderWidth: 2,
                backgroundColor: rgb('--color-accent', 0.18),
                tension: 0.3,
                fill: true,
                pointRadius: 3,
                pointHoverRadius: 5,
                pointBackgroundColor: corLinha,
                pointBorderColor: rgb('--color-surface-primary'),
                pointBorderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: { mode: 'index', intersect: false },
            plugins: { legend: { display: false }, tooltip: tooltipPadrao },
            scales: {
                x: { grid: { display: false }, border: { color: rgb('--color-border-default') } },
                y: { beginAtZero: true, ticks: { precision: 0 }, grid: { color: rgb('--color-border-default') }, border: { display: false } }
            }
        }
    });

    // ===== Gráfico de pizza: alunos por curso (ou por módulo) =====
    var canvasPizza = document.getElementById('pieGraphCursos');
    new Chart(canvasPizza, {
        type: 'pie',
        data: {
            labels: dados.pieLabels,
            datasets: [{
                data: dados.pieData,
                backgroundColor: coresPizza,
                borderColor: rgb('--color-surface-primary'),
                borderWidth: 2,
                hoverOffset: 4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: Object.assign({}, tooltipPadrao, {
                    callbacks: {
                        title: function () { return ''; },
                        label: function (item) { return item.label + ': ' + item.formattedValue; }
                    }
                })
            }
        }
    });

    // Com um curso escolhido, a fatia mostra só o nome do módulo (o curso já está no select)
    function rotuloFatia(d) { return d.modulo ? d.modulo : d.curso; }

    // Legenda HTML da pizza (ao abrir a página e depois de cada fetch)
    function desenharLegendaPizza() {
        var chart = Chart.getChart('pieGraphCursos');
        var lista = document.getElementById('pieLegenda');
        var valores = chart.data.datasets[0].data;
        var total = valores.reduce(function (soma, v) { return soma + v; }, 0);
        lista.replaceChildren();
        chart.data.labels.forEach(function (rotulo, i) {
            var item = document.createElement('li');
            item.className = 'flex items-start gap-2.5';
            var cor = document.createElement('span');
            cor.className = 'mt-1 h-2.5 w-2.5 shrink-0 rounded-sm';
            cor.style.backgroundColor = coresPizza[i % coresPizza.length];
            var texto = document.createElement('span');
            texto.className = 'min-w-0 flex-1 break-words leading-snug text-ink-soft';
            texto.textContent = rotulo;
            var numeros = document.createElement('span');
            numeros.className = 'shrink-0 text-right tabular-nums text-ink';
            var pct = total > 0 ? Math.round(valores[i] / total * 100) : 0;
            numeros.innerHTML = '<span class="font-medium">' + valores[i] + '</span> <span class="text-ink-muted">' + pct + '%</span>';
            item.append(cor, texto, numeros);
            lista.append(item);
        });
        canvasPizza.setAttribute('aria-label', descrever('Gráfico de pizza: alunos por curso', chart.data.labels, valores));
    }

    desenharLegendaPizza();

    // ===== Fetch do gráfico em linha (DashboardController.GetLineGraphData) =====
    async function atualizarGraficoLinha() {
        var card = document.getElementById('cardLinha');
        var tempo = document.getElementById('selectTempoLinha').value;
        var curso = document.getElementById('selectCursoLinha').value;
        carregando(card, true);
        try {
            var resp = await fetch(dados.urlLinha + '?tempo=' + encodeURIComponent(tempo) + '&cursoId=' + encodeURIComponent(curso));
            if (!resp.ok) throw new Error('HTTP ' + resp.status);
            var pontos = await resp.json();
            var chart = Chart.getChart('lineGraphAlunos');
            chart.data.labels = pontos.map(function (d) { return new Date(d.dataRegistro).toLocaleDateString('pt-BR', { month: '2-digit', year: 'numeric' }); });
            chart.data.datasets[0].data = pontos.map(function (d) { return d.totalAlunos; });
            chart.update();
            canvasLinha.setAttribute('aria-label', descrever('Gráfico de linha: alunos cadastrados por mês', chart.data.labels, chart.data.datasets[0].data));
        } catch (e) {
            falhou('Não foi possível atualizar o histórico de alunos. O gráfico continua com os dados anteriores.');
        } finally {
            carregando(card, false);
        }
    }

    document.getElementById('selectTempoLinha').addEventListener('change', atualizarGraficoLinha);
    document.getElementById('selectCursoLinha').addEventListener('change', atualizarGraficoLinha);

    // ===== Fetch do gráfico de pizza (DashboardController.GetPieGraphData) =====
    document.getElementById('selectCursoPizza').addEventListener('change', async function () {
        var card = document.getElementById('cardPizza');
        var cursoId = document.getElementById('selectCursoPizza').value;
        carregando(card, true);
        try {
            var resp = await fetch(dados.urlPizza + '?cursoId=' + encodeURIComponent(cursoId));
            if (!resp.ok) throw new Error('HTTP ' + resp.status);
            var fatias = await resp.json();
            var chart = Chart.getChart('pieGraphCursos');
            chart.data.labels = fatias.map(rotuloFatia);
            chart.data.datasets[0].data = fatias.map(function (d) { return d.quantidadeAlunos; });
            chart.update();
            desenharLegendaPizza();
        } catch (e) {
            falhou('Não foi possível atualizar o gráfico de alunos por curso. O gráfico continua com os dados anteriores.');
        } finally {
            carregando(card, false);
        }
    });
})();
