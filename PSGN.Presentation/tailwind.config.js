/** REVISÃO: configuração do Tailwind ligada à nova identidade cromática. Cada classe de cor (bg-brand, text-ink, border-line...) aponta para um PAPEL de
 *  Styles/tokens.css (--color-*); nenhum valor de cor mora aqui. Os apelidos antigos (brand, ink, line, card, page, success, danger...) foram MANTIDOS,
 *  então as views não precisam mudar quando a identidade muda. */
const token = (nome) => `rgb(var(--${nome}) / <alpha-value>)`; // monta a cor a partir do tripleto RGB do token, mantendo o suporte a transparência (bg-ink/5)

module.exports = {
  content: [
    './Views/**/*.cshtml', // todas as views e partials (o Tailwind procura as classes escritas nelas)
    './wwwroot/js/**/*.js' // os scripts próprios também escrevem classes (ex.: a legenda do gráfico)
  ],
  // O Tailwind descarta as classes de componente que não aparecem ESCRITAS por inteiro nas views/JS. Estas variantes são montadas em tempo de execução
  // (ex.: a partial _Alert gera "alert-" + o tipo, a partial _Kpi gera "kpi-icon-" + o tom) ou ainda não têm uso (btn-danger), então ficam sempre no CSS gerado
  safelist: [
    { pattern: /^alert-(success|danger|warning|info)$/ }, // variantes do aviso
    { pattern: /^btn-(primary|secondary|ghost|danger|sm|icon)$/ }, // variantes do botão
    { pattern: /^kpi-icon-(accent|highlight)$/ }, // tom do tile do indicador
    { pattern: /^badge-(accent|highlight)$/ }, // variantes da etiqueta
    'field-error', 'field-required', 'link' // validação de formulário e link de texto: parte do padrão, ainda sem uso nas telas atuais
  ],
  theme: {
    extend: {
      colors: {
        page: token('color-surface-page'), // bg-page: fundo da página
        card: token('color-surface-primary'), // bg-card: cards, tabelas e campos
        subtle: token('color-surface-secondary'), // bg-subtle: áreas de apoio
        support: token('color-surface-support'), // bg-support: etiquetas e áreas de apoio (Pale Sky)
        surface: { hover: token('color-surface-hover') }, // bg-surface-hover: linha de tabela em hover
        line: { DEFAULT: token('color-border-default'), control: token('color-border-control') }, // border-line (decorativa) e border-line-control (componentes, 3:1)
        ink: { DEFAULT: token('color-text-primary'), soft: token('color-text-secondary'), muted: token('color-text-muted') }, // text-ink, text-ink-soft, text-ink-muted
        brand: { DEFAULT: token('color-brand-primary'), dark: token('color-brand-primary-hover'), pressed: token('color-brand-primary-pressed'), soft: token('color-brand-primary-soft'), tint: token('color-brand-primary-tint') }, // identidade (Twilight Indigo)
        accent: { DEFAULT: token('color-accent'), strong: token('color-accent-strong'), icon: token('color-accent-icon'), soft: token('color-accent-soft'), tint: token('color-accent-tint') }, // destaque principal (Sky Surge)
        highlight: { DEFAULT: token('color-highlight'), graphic: token('color-highlight-graphic'), soft: token('color-highlight-soft'), tint: token('color-highlight-tint') }, // destaque secundário (Vibrant Coral)
        notice: { DEFAULT: token('color-notice'), soft: token('color-notice-soft'), tint: token('color-notice-tint'), text: token('color-notice-text'), icon: token('color-notice-icon') }, // atenção (Banana Cream)
        interactive: token('color-interactive-hover'), // hover neutro, usado com transparência (bg-interactive/[0.06])
        overlay: token('color-overlay'), // véu atrás de painéis e modais
        sidebar: { bg: token('color-sidebar-bg'), hover: token('color-sidebar-hover'), active: token('color-sidebar-active'), text: token('color-sidebar-text'), muted: token('color-sidebar-muted'), icon: token('color-sidebar-icon'), line: token('color-sidebar-line') }, // sidebar (fundo indigo)
        success: { DEFAULT: token('color-feedback-success'), soft: token('color-feedback-success-soft') }, // feedback de sucesso
        warning: { DEFAULT: token('color-notice-text'), soft: token('color-notice-soft'), icon: token('color-notice-icon') }, // feedback de atenção (texto escuro sobre Banana)
        danger: { DEFAULT: token('color-feedback-danger'), soft: token('color-feedback-danger-soft') } // feedback de erro e ação destrutiva
      },
      fontFamily: {
        sans: ['var(--font-sans)'] // classe font-sans = Inter
      },
      // Escala tipográfica fechada: 12 / 13 / 14 / 16 / 20 / 24 / 28 px. O texto base do sistema é 14px
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
