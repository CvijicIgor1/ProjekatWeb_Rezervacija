using System.Collections.Generic;

namespace ProjekatWeb.Models
{
    public class Smestajni_objekat
    {
        public string Id { get; set; }
        public string Naziv { get; set; }
        public string Tip { get; set; }
        public string Opis { get; set; }
        public string Adresa { get; set; }
        public string Grad { get; set; }
        public double Cena_po_noci { get; set; }   
        public int Maksimalan_broj_gostiju { get; set; }
        public string Slika { get; set; }
        public string Datum_postavljanja_oglasa { get; set; } // dd/MM/yyyy
        public bool Dostupnost { get; set; } = true;
        public bool Obrisan { get; set; } = false;
        public string DomacinKorisnickoIme { get; set; }
        public List<string> RecenzijaIds { get; set; } = new List<string>();
    }
}