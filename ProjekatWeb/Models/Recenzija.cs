using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjekatWeb.Models
{
    public class Recenzija
    {
        public string Id { get; set; }
        public string SmestajniObjekatId { get; set; }
        public string RecenzentKorisnickoIme { get; set; }
        public string Naslov { get; set; }
        public string Sadrzaj { get; set; }
        public int Ocena { get; set; }  // 1-5
        public string Slika { get; set; }
        public StatusRecenzije Status { get; set; } = StatusRecenzije.Kreirana;
        public bool Obrisana { get; set; } = false;
    }

    public enum StatusRecenzije
    {
        Kreirana,
        Odobrena,
        Odbijena
    }
}