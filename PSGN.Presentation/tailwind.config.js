/** REVISÃO: configuração do Tailwind reescrita. Antes só tinha a cor "brand"; agora liga as classes do Tailwind aos tokens de Styles/tokens.css
 *  (cores, fonte, escala de texto, raios, sombra), então o sistema inteiro muda mudando só os tokens. */
const token = (nome) => `rgb(var(--${nome}) / <alpha-value>)`; // monta a cor a partir do tripleto RGB do token, mantendo o suporte a transparência (bg-ink/5)

module.exports = {
  content: [
    './Views/**/*.cshtml', // todas as views e partials (o Tailwind procura as classes escritas nelas)
    './wwwroot/js/**/*.js' // os scripts próprios também escrevem classes (ex.: a legenda do gráfico)
  ],
  // O Tailwind descarta as classes de componente que não aparecem ESCRITAS por inteiro nas views/JS. Estas variantes são montadas em tempo de execução
  // (ex.: a partial _Alert gera "alert-" + o tipo) ou ainda não têm uso (btn-danger), então ficam sempre no CSS gerado
  safelist: [
    { pattern: /^alert-(success|danger|warning)$/ }, // variantes do aviso
    { pattern: /^btn-(primary|secondary|ghost|danger|sm|icon)$/ } // variantes do botão
  ],
  theme: {
    extend: {
      colors: {
        page: token('page'), // bg-page
        card: token('card'), // bg-card
        line: { DEFAULT: token('line'), strong: token('line-strong') }, // border-line, border-line-strong
        ink: { DEFAULT: token('ink'), soft: token('ink-2'), muted: token('ink-3') }, // text-ink, text-ink-soft, text-ink-muted
        brand: { DEFAULT: token('brand'), dark: token('brand-dark'), soft: token('brand-soft') }, // acento
        success: { DEFAULT: token('success'), soft: token('success-soft') }, // feedback de sucesso
        warning: { DEFAULT: token('warning'), soft: token('warning-soft') }, // feedback de aviso
        danger: { DEFAULT: token('danger'), soft: token('danger-soft') } // feedback de erro e ação destrutiva
      },
      fontFamily: {
        sans: ['var(--font-sans)'] // classe font-sans = Inter
      },
      // Escala tipográfica fechada: 12 / 13 / 14 / 16 / 20 / 24 / 28 px. O texto base do sistema passa a ser 14px (era 16px)
      fontSize: {
        xs: ['0.75rem', { lineHeight: '1rem' }], // 12px: rótulos e metadados
        sm: ['0.8125rem', { lineHeight: '1.25rem' }], // 13px: tabelas e controles
        base: ['0.875rem', { lineHeight: '1.375rem' }], // 14px: corpo
        lg: ['1rem', { lineHeight: '1.5rem' }], // 16px: títulos de card
        xl: ['1.25rem', { lineHeight: '1.75rem' }], // 20px
        '2xl': ['1.5rem', { lineHeight: '2rem' }], // 24px: título de página
        '3xl': ['1.75rem', { lineHeight: '2.25rem' }] // 28px: números dos indicadores
      },
      borderRadius: {
        control: 'var(--radius-control)', // rounded-control
        card: 'var(--radius-card)' // rounded-card
      },
      boxShadow: {
        pop: 'var(--shadow-pop)' // shadow-pop: única sombra do sistema
      },
      maxWidth: {
        content: 'var(--content-max)' // max-w-content
      }
    }
  },
  plugins: []
};
