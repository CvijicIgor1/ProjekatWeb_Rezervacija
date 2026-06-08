using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjekatWeb.Models
{
    public class Rezervacija
    {
        public string Id { get; set; }  
        public string SmestajniObjekatId { get; set; }
        public string Gost { get; set; }
        public string DatumPrijave { get; set; }
        public string DatumOdjave { get; set; }
        public int BrojGostiju { get; set; }  
        public double UkupnaCena { get; set; }
        public StatusRezervacije Status { get; set; } = StatusRezervacije.Kreirana;
        public bool Obrisana { get; set; } = false;
    }

    public enum StatusRezervacije
    {
        Kreirana,
        Odobrena,
        Otkazana,
        Zavrsena
    }
}