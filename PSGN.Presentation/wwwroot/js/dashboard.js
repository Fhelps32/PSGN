// REVISÃO: arquivo novo. Todo este código estava dentro de um <script> na view Dashboard/Index.cshtml; agora fica num arquivo próprio
// (o navegador guarda em cache, e a view fica só com HTML). Os dados e as URLs chegam por <script id="dashboard-data" type="application/json">.
(function () { // função que roda na hora e isola as variáveis, para não vazarem para o resto do site
    'use strict'; // modo estrito: o JavaScript avisa de erros bobos em vez de ignorá-los

    var bloco = document.getElementById('dashboard-data'); // bloco JSON que a view escreveu
    if (!bloco || typeof Chart === 'undefined') return; // sem dados ou sem o Chart.js carregado não há o que desenhar
    var dados = JSON.parse(bloco.textContent); // { lineLabels, lineData, pieLabels, pieData, urlLinha, urlPizza }

    // Monta o texto alternativo do gráfico, lido por leitores de tela (o <canvas> sozinho é uma imagem muda para eles)
    function descrever(titulo, rotulos, valores) { // titulo = frase de abertura; rotulos e valores = os pontos do gráfico
        var itens = rotulos.map(function (rotulo, i) { return rotulo + ': ' + valores[i]; }); // "01/2026: 3"
        return titulo + (itens.length ? '. ' + itens.join('; ') : '. Sem dados.'); // junta tudo numa frase só
    } // fim do descrever

    // ===== Gráfico em linha: alunos cadastrados por mês =====
    var canvasLinha = document.getElementById('lineGraphAlunos'); // <canvas> do gráfico em linha
    canvasLinha.setAttribute('aria-label', descrever('Gráfico de linha: alunos cadastrados por mês', dados.lineLabels, dados.lineData)); // texto alternativo com os valores reais
    new Chart(canvasLinha, { // desenha no <canvas id="lineGraphAlunos">
        type: 'line', // gráfico de linha
        data: { // dados
            labels: dados.lineLabels, // eixo X
            datasets: [{ // uma única série
                label: 'Alunos', // nome da série (aparece no tooltip)
                data: dados.lineData, // eixo Y
                borderColor: '#c96442', // cor da linha (a mesma do PSG)
                backgroundColor: 'rgba(201, 100, 66, 0.12)', // cor do preenchimento abaixo da linha
                tension: 0.3, // suaviza a curva
                fill: true, // preenche abaixo da linha
                pointRadius: 3 // tamanho dos pontos
            }] // fim da série
        }, // fim dos dados
        options: { // opções
            responsive: true, // acompanha o tamanho do container
            maintainAspectRatio: false, // usa a altura do container (h-72) em vez de uma proporção fixa
            plugins: { legend: { display: false } }, // esconde a legenda (só há uma série)
            scales: { y: { beginAtZero: true, ticks: { precision: 0 } } } // eixo Y começa no 0 e só mostra inteiros
        } // fim das opções
    }); // fim do gráfico em linha

    // ===== Gráfico de pizza: alunos por curso (ou por módulo) =====
    var coresPizza = ['#c96442', '#f2b774', '#7c9d96', '#5b7f9e', '#a68a64', '#94a3b8']; // cores das fatias (as mesmas do PSG); ficam numa variável porque a legenda HTML usa as mesmas
    var canvasPizza = document.getElementById('pieGraphCursos'); // <canvas> do gráfico de pizza
    canvasPizza.setAttribute('aria-label', descrever('Gráfico de pizza: alunos por curso', dados.pieLabels, dados.pieData)); // texto alternativo com os valores reais
    new Chart(canvasPizza, { // desenha no <canvas id="pieGraphCursos">
        type: 'pie', // gráfico de pizza
        data: { // dados
            labels: dados.pieLabels, // nome de cada fatia (aparece no tooltip e na legenda HTML)
            datasets: [{ // uma única série
                data: dados.pieData, // tamanho das fatias
                backgroundColor: coresPizza // o Chart.js repete as cores em ciclo quando há mais fatias que cores
            }] // fim da série
        }, // fim dos dados
        options: { // opções
            responsive: true, // acompanha o tamanho do container
            maintainAspectRatio: false, // usa a altura do container (h-48)
            plugins: { legend: { display: false } } // legenda do Chart.js desligada; quem mostra é a <ul id="pieLegenda">
        } // fim das opções
    }); // fim do gráfico de pizza

    // Legenda HTML da pizza: no PSG a legenda era "Curso - Módulo"; no PSGN os nomes são longos e espremiam a pizza.
    // Como o curso já aparece no select, a fatia de módulo mostra só o nome do módulo
    function rotuloFatia(d) { return d.modulo ? d.modulo : d.curso; } // monta a legenda a partir do JSON do AJAX (módulo só existe com curso escolhido)

    // Monta a legenda HTML a partir do que está no gráfico (chamada ao abrir a página e depois de cada AJAX)
    function desenharLegendaPizza() {
        var chart = Chart.getChart('pieGraphCursos'); // gráfico já desenhado
        var lista = document.getElementById('pieLegenda'); // <ul> da legenda
        lista.replaceChildren(); // apaga a legenda anterior
        chart.data.labels.forEach(function (rotulo, i) { // um item por fatia
            var item = document.createElement('li'); // linha da legenda
            item.className = 'flex items-start gap-2'; // quadradinho, texto e número lado a lado
            var cor = document.createElement('span'); // quadradinho com a cor da fatia
            cor.className = 'mt-0.5 h-2.5 w-2.5 shrink-0 rounded-sm'; // tamanho e alinhamento com a 1ª linha do texto
            cor.style.backgroundColor = coresPizza[i % coresPizza.length]; // mesma regra de ciclo de cores do Chart.js
            var texto = document.createElement('span'); // nome do curso/módulo
            texto.className = 'flex-1 leading-snug'; // ocupa o espaço que sobra e quebra linha se for longo
            texto.textContent = rotulo; // textContent (e não innerHTML) para o nome nunca ser interpretado como HTML
            var valor = document.createElement('span'); // quantidade de alunos da fatia
            valor.className = 'tabular-nums font-semibold text-slate-800'; // números alinhados e em destaque
            valor.textContent = chart.data.datasets[0].data[i]; // valor da fatia
            item.append(cor, texto, valor); // monta a linha
            lista.append(item); // coloca a linha na legenda
        }); // fim do forEach
        canvasPizza.setAttribute('aria-label', descrever('Gráfico de pizza: alunos por curso', chart.data.labels, chart.data.datasets[0].data)); // mantém o texto alternativo igual ao que está na tela
    } // fim do desenharLegendaPizza

    desenharLegendaPizza(); // desenha a legenda da primeira renderização

    // ===== AJAX do gráfico em linha =====
    async function atualizarGraficoLinha() { // chamado quando o período ou o curso muda
        var tempo = document.getElementById('selectTempoLinha').value; // quantidade de meses escolhida
        var curso = document.getElementById('selectCursoLinha').value; // id do curso ("" = todos)

        var resp = await fetch(dados.urlLinha + '?tempo=' + encodeURIComponent(tempo) + '&cursoId=' + encodeURIComponent(curso)); // pede os pontos novos ao DashboardController
        if (!resp.ok) return; // se deu erro no servidor, mantém o gráfico como está

        var pontos = await resp.json(); // formato: [{ totalAlunos: 10, dataRegistro: "2026-01-01T00:00:00" }, ...]
        var chart = Chart.getChart('lineGraphAlunos'); // pega o gráfico já desenhado
        chart.data.labels = pontos.map(function (d) { return new Date(d.dataRegistro).toLocaleDateString('pt-BR', { month: '2-digit', year: 'numeric' }); }); // novos rótulos MM/yyyy
        chart.data.datasets[0].data = pontos.map(function (d) { return d.totalAlunos; }); // novos valores
        chart.update(); // redesenha com animação
        canvasLinha.setAttribute('aria-label', descrever('Gráfico de linha: alunos cadastrados por mês', chart.data.labels, chart.data.datasets[0].data)); // atualiza o texto alternativo
    } // fim do atualizarGraficoLinha

    document.getElementById('selectTempoLinha').addEventListener('change', atualizarGraficoLinha); // troca de período
    document.getElementById('selectCursoLinha').addEventListener('change', atualizarGraficoLinha); // troca de curso

    // ===== AJAX do gráfico de pizza =====
    document.getElementById('selectCursoPizza').addEventListener('change', async function () { // quando o curso da pizza muda
        var cursoId = document.getElementById('selectCursoPizza').value; // id do curso ("" = todos)
        var resp = await fetch(dados.urlPizza + '?cursoId=' + encodeURIComponent(cursoId)); // pede as fatias novas ao DashboardController
        if (!resp.ok) return; // se deu erro no servidor, mantém o gráfico como está

        var fatias = await resp.json(); // formato: [{ curso: "...", modulo: "..." | null, quantidadeAlunos: 5 }, ...]
        var chart = Chart.getChart('pieGraphCursos'); // pega o gráfico já desenhado
        chart.data.labels = fatias.map(rotuloFatia); // nova legenda
        chart.data.datasets[0].data = fatias.map(function (d) { return d.quantidadeAlunos; }); // novos tamanhos
        chart.update(); // redesenha com animação
        desenharLegendaPizza(); // atualiza a legenda HTML (e o texto alternativo) com as fatias novas
    }); // fim do evento da pizza
})(); // fim da função que roda na hora
