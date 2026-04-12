using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libary.Model.Project
{
    /// <summary>
    /// Projekt feladathoz tartozó részfeladat (TODO) adatátviteli modell.
    /// </summary>
    public class TodoDto
    {
        /// <summary>
        /// TODO azonosító.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// TODO szövege/rövid leírása.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Opcionális részösszeg, amely a task budgetből levonható.
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Készre jelölt státusz.
        /// </summary>
        public bool Completed { get; set; }

        /// <summary>
        /// Az aktuális felelős user azonosítója.
        /// </summary>
        public string? WorkerId { get; set; }
    }
}
