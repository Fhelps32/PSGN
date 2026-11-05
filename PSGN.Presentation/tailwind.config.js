/** Tailwind do PSGN: cada classe de cor (bg-brand, text-ink, border-line...) aponta para um papel de Styles/tokens.css. Nenhum valor de cor mora aqui. */
const token = (nome) => `rgb(var(--${nome}) / <alpha-value>)`;

module.exports = {
  content: [
    './Views/**/*.cshtml',
    './wwwroot/js/**/*.js'
  ],
  // Classes montadas em tempo de execução (ex.: "alert-" + tipo no _Alert, "kpi-icon-" + tom no _Kpi) ou ainda sem uso: o Tailwind as descartaria
  safelist: [
    { pattern: /^alert-(success|danger|warning|info)$/ },
    { pattern: /^btn-(primary|secondary|ghost|danger|sm|icon)$/ },
    { pattern: /^kpi-icon-(accent|highlight)$/ },
    { pattern: /^badge-(accent|highlight)$/ },
    'field-error', 'field-required', 'link'
  ],
  theme: {
    extend: {
      colors: {
        page: token('color-surface-page'),
        card: token('color-surface-primary'),
        subtle: token('color-surface-secondary'),
        support: token('color-surface-support'),
        surface: { hover: token('color-surface-hover') },
        line: { DEFAULT: token('color-border-default'), control: token('color-border-control') },
        ink: { DEFAULT: token('color-text-primary'), soft: token('color-text-secondary'), muted: token('color-text-muted') },
        brand: { DEFAULT: token('color-brand-primary'), dark: token('color-brand-primary-hover'), pressed: token('color-brand-primary-pressed'), soft: token('color-brand-primary-soft'), tint: token('color-brand-primary-tint') },
        accent: { DEFAULT: token('color-accent'), strong: token('color-accent-strong'), icon: token('color-accent-icon'), soft: token('color-accent-soft'), tint: token('color-accent-tint') },
        highlight: { DEFAULT: token('color-highlight'), graphic: token('color-highlight-graphic'), soft: token('color-highlight-soft'), tint: token('color-highlight-tint') },
        notice: { DEFAULT: token('color-notice'), soft: token('color-notice-soft'), tint: token('color-notice-tint'), text: token('color-notice-text'), icon: token('color-notice-icon') },
        interactive: token('color-interactive-hover'),
        overlay: token('color-overlay'),
        sidebar: { bg: token('color-sidebar-bg'), hover: token('color-sidebar-hover'), active: token('color-sidebar-active'), text: token('color-sidebar-text'), muted: token('color-sidebar-muted'), icon: token('color-sidebar-icon'), line: token('color-sidebar-line') },
        success: { DEFAULT: token('color-feedback-success'), soft: token('color-feedback-success-soft') },
        warning: { DEFAULT: token('color-notice-text'), soft: token('color-notice-soft'), icon: token('color-notice-icon') },
        danger: { DEFAULT: token('color-feedback-danger'), soft: token('color-feedback-danger-soft') }
      },
      fontFamily: {
        sans: ['var(--font-sans)']
      },
      // Escala tipográfica fechada: 12 / 13 / 14 / 16 / 20 / 24 / 28 px. O texto base do sistema é 14px
      fontSize: {
        xs: ['0.75rem', { lineHeight: '1rem' }],
        sm: ['0.8125rem', { lineHeight: '1.25rem' }],
        base: ['0.875rem', { lineHeight: '1.375rem' }],
        lg: ['1rem', { lineHeight: '1.5rem' }],
        xl: ['1.25rem', { lineHeight: '1.75rem' }],
        '2xl': ['1.5rem', { lineHeight: '2rem' }],
        '3xl': ['1.75rem', { lineHeight: '2.25rem' }]
      },
      borderRadius: {
        control: 'var(--radius-control)',
        card: 'var(--radius-card)'
      },
      boxShadow: {
        pop: 'var(--shadow-pop)'
      },
      maxWidth: {
        content: 'var(--content-max)'
      }
    }
  },
  plugins: []
};
