using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoFinal_PPII_G6.Models
{
    public class Asistencia
    {
        public Asistencia()
        {
        }

        public Asistencia(int id, int estudianteId, int claseId, bool presente)
        {
            this.Id = id;
            this.EstudianteId = estudianteId;
            this.ClaseId = claseId;
            this.Presente = presente;
        }

        public int Id
        {
            get;
            set;
        }
        public int EstudianteId
        {
            get;
            set;
        }

        public int ClaseId
        {
            get;
            set;
        }

        public bool Presente
        {
            get;
            set;
        }
    }
}