using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using ProjekatWeb.Models;

namespace ProjekatWeb.Services
{
    public class RezervacijaService
    {
        private string putanja => HttpContext.Current.Server.MapPath("~/App_Data/rezervacija.json");

        public List<Rezervacija> GetSve()
        {
            if (!File.Exists(putanja)) 
                return new List<Rezervacija>();
            string json = File.ReadAllText(putanja);
            return JsonConvert.DeserializeObject<List<Rezervacija>>(json) ?? new List<Rezervacija>();
        }

        public void Sacuvaj(List<Rezervacija> rezervacije)
        {
            string json = JsonConvert.SerializeObject(rezervacije, Formatting.Indented);
            File.WriteAllText(putanja, json);
        }

        public Rezervacija NadjiPoId(string id)
        {
            return GetSve().FirstOrDefault(r => r.Id == id && !r.Obrisana);
        }

        public List<Rezervacija> GetZaGosta(string korisnickoIme)
        {
            return GetSve().Where(r => r.Gost == korisnickoIme && !r.Obrisana).ToList();
        }

        public void Dodaj(Rezervacija r)
        {
            var lista = GetSve();
            lista.Add(r);
            Sacuvaj(lista);
        }

        public void Azuriraj(Rezervacija izmenjena)
        {
            var lista = GetSve();
            int index = lista.FindIndex(r => r.Id == izmenjena.Id);
            if (index != -1) 
                lista[index] = izmenjena;
            Sacuvaj(lista);
        }

        public void ObrisiLogicki(string id)
        {
            var lista = GetSve();
            var r = lista.FirstOrDefault(x => x.Id == id);
            if (r != null) 
                r.Obrisana = true;
            Sacuvaj(lista);
        }
    }
}