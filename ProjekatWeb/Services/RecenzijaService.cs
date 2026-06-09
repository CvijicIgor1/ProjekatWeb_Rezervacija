using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using ProjekatWeb.Models;

namespace ProjekatWeb.Services
{
    public class RecenzijaService
    {
        private string putanja = HttpContext.Current.Server.MapPath("~/App_Data/recenzija.json");

        public List<Recenzija> GetSve()
        {
            if (!File.Exists(putanja)) 
                return new List<Recenzija>();
            string json = File.ReadAllText(putanja);
            return JsonConvert.DeserializeObject<List<Recenzija>>(json) ?? new List<Recenzija>();
        }

        public void Sacuvaj(List<Recenzija> recenzije)
        {
            string json = JsonConvert.SerializeObject(recenzije, Formatting.Indented);
            File.WriteAllText(putanja, json);
        }

        public Recenzija NadjiPoId(string id)
        {
            return GetSve().FirstOrDefault(r => r.Id == id && !r.Obrisana);
        }

        public List<Recenzija> GetZaObjekat(string objekatId)
        {
            return GetSve().Where(r => r.SmestajniObjekatId == objekatId && !r.Obrisana).ToList();
        }

        public void Dodaj(Recenzija r)
        {
            var lista = GetSve();
            lista.Add(r);
            Sacuvaj(lista);
        }

        public void Azuriraj(Recenzija izmenjena)
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