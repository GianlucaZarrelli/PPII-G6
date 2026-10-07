using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoFinal_PPII_G6.Models
{
    public class TrabajoPractico
    {
        public int Id { get; set; }

        public int ComisionId { get; set; }

        public string Titulo { get; set; } = string.Empty;

        public DateTime FechaEntrega { get; set; }

        public TrabajoPractico() { }

        public TrabajoPractico(int id, int comisionId, string titulo, DateTime fechaEntrega)
        {
            Id = id;
            ComisionId = comisionId;
            Titulo = titulo;
            FechaEntrega = fechaEntrega;
        }
    }
}
