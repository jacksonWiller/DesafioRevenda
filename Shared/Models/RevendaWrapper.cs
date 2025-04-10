using System.Collections.Generic;

namespace Shared.Models
{
    public class RevendaWrapper
    {
        public RevendaWrapper()
        {
            this.Revendas = new List<Revenda>();
        }

        public RevendaWrapper(List<Revenda> revendas)
        {
            this.Revendas = revendas;
        }

        public List<Revenda> Revendas { get; set; }
    }
}