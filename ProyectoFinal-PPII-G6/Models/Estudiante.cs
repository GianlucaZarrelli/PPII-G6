using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoFinal_PPII_G6.Models
{
    public class Estudiante
    {
        public Estudiante()
        {
        }

        public Estudiante(int id, string dni, string nombre, string apellido)
        {
            this.Id = id;
            this.Dni = dni;
            this.Nombre = nombre;
            this.Apellido = apellido;
        }

        
        public int Id
        {
            get;
            set;
        }

        public string Dni
        {
            get;
            set;
        }

        public string Nombre
        {
            get;
            set;
        }

        public string Apellido
        {
            get;
            set;
        }

        public string NombreCompleto
        {
            get
            {
                return $"{Nombre} {Apellido}";
            }
        }
    }
}
