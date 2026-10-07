using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoFinal_PPII_G6.Models
{
    public class Comision
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public Comision() { }

        public Comision(int id, string nombre)
        {
            Id = id;
            Nombre = nombre;
        }
    }
}
