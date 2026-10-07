using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoFinal_PPII_G6.Models
{
    public class Entrega
    {
        public int Id { get; set; }
        public int EstudianteId { get; set; }
        public int TrabajoPracticoId { get; set; }
        public bool Entregado { get; set; }
        
        public Entrega() { }

        public Entrega(int id, int estudianteId, int trabajoPracticoId, bool entregado)
        {
            Id = id;
            EstudianteId = estudianteId;
            TrabajoPracticoId = trabajoPracticoId;
            Entregado = entregado;
        }
    }
}
