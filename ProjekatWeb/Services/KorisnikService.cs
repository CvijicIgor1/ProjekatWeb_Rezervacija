using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using ProjekatWeb.Models;

namespace ProjekatWeb.Services
{
    public class KorisnikService
    {
        private string putanja => HttpContext.Current.Server.MapPath("~/App_Data/korisnici.json");


        public List<Korisnik> GetSvi()
        {
            if (!File.Exists(putanja)) 
                return new List<Korisnik>();
            string json = File.ReadAllText(putanja);
            return JsonConvert.DeserializeObject<List<Korisnik>>(json) ?? new List<Korisnik>();
        }

        public void SacuvajSve(List<Korisnik> korisnici)
        {
            string json = JsonConvert.SerializeObject(korisnici, Formatting.Indented);
            File.WriteAllText(putanja, json);
        }

        public Korisnik NadjiPoKorisnickomImenu(string korisnickoIme)
        {
            return GetSvi().FirstOrDefault(k => k.KorisnickoIme == korisnickoIme && !k.Obrisan);
        }

        public void Dodaj(Korisnik k)
        {
            var lista = GetSvi();
            lista.Add(k);
            SacuvajSve(lista);
        }

        public void Azuriraj(Korisnik izmenjen)
        {
            var lista = GetSvi();
            int index = lista.FindIndex(k => k.KorisnickoIme == izmenjen.KorisnickoIme);
            if (index != -1) 
                lista[index] = izmenjen;
            SacuvajSve(lista);
        }

        public void ObrisiLogicki(string korisnickoIme)
        {
            var lista = GetSvi();
            var k = lista.FirstOrDefault(x => x.KorisnickoIme == korisnickoIme);
            if (k != null) 
                k.Obrisan = true;
            SacuvajSve(lista);
        }
    }
}