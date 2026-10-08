using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSGN.Infra.Integracoes.ExternoServices
{
    public class ExternoServiceOpcoes
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public int ParentCategoryId { get; set; }
    }
}
