using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjekatWeb.Models
{
    public class Korisnik
    {
        public string KorisnickoIme { get; set; }  
        public string Lozinka { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Email { get; set; }
        public string DatumRodjenja { get; set; }
        public string Pol { get; set; }
        public TipKorisnika TipKorisnika { get; set; } 
        public bool Obrisan { get; set; } = false;

        public List<string> RezervacijaIds { get; set; } = new List<string>();
        public List<string> ObjektiIds { get; set; } = new List<string>();
    }

    public enum TipKorisnika
    {
        Gost,
        Domacin,
        Administrator  
    }
}