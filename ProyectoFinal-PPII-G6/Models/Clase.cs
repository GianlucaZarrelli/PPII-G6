using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoFinal_PPII_G6.Models
{
    public class Clase
    {
        public Clase()
        {
        }

        public Clase(int id, DateTime fecha, string descripcion)
        {
            this.Id = id;
            this.Fecha = fecha;
            this.Descripcion = descripcion;
        }

        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Descripcion { get; set; }
    }
}
