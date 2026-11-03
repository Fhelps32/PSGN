/** REVISÃO: configuração do Tailwind. A cor "brand" e a fonte são as mesmas que estavam no <script> do _Layout quando o Tailwind vinha do Play CDN. */
module.exports = {
  content: [
    './Views/**/*.cshtml', // todas as views e partials (o Tailwind procura as classes escritas nelas)
    './wwwroot/js/**/*.js' // os scripts próprios (sidebar.js, dashboard.js, alunos.js) também escrevem classes
  ],
  theme: {
    extend: {
      colors: {
        brand: {
          DEFAULT: '#2563eb', // azul principal
          dark: '#1d4ed8', // azul do hover dos botões
          light: '#eff4ff' // fundo claro do item ativo da sidebar
        }
      },
      fontFamily: {
        sans: ['"Segoe UI"', 'system-ui', 'sans-serif'] // fonte padrão do sistema
      }
    }
  },
  plugins: []
};
