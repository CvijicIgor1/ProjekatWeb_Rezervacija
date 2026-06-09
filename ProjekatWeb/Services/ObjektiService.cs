using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using ProjekatWeb.Models;

namespace ProjekatWeb.Services
{
    public class ObjektiService
    {
        private string putanja = HttpContext.Current.Server.MapPath("~/App_Data/objekti.json");


        public List<Smestajni_objekat> GetSvi()
        {
            if (!File.Exists(putanja))
                return new List<Smestajni_objekat>();
            string json = File.ReadAllText(putanja);
            return JsonConvert.DeserializeObject<List<Smestajni_objekat>>(json) ?? new List<Smestajni_objekat>();
        }

        public void SacuvajSve(List<Smestajni_objekat> objekti)
        {
            string json = JsonConvert.SerializeObject(objekti, Formatting.Indented);
            File.WriteAllText(putanja, json);
        }

        public Smestajni_objekat NadjiPoId(string id)
        {
            return GetSvi().FirstOrDefault(o => o.Id == id && !o.Obrisan);
        }

        public void Dodaj(Smestajni_objekat o)
        {
            var lista = GetSvi();
            lista.Add(o);
            SacuvajSve(lista);
        }

        public void Azuriraj(Smestajni_objekat izmenjen)
        {
            var lista = GetSvi();
            int index = lista.FindIndex(k => k.Id == izmenjen.Id);
            if (index != -1)
                lista[index] = izmenjen;
            SacuvajSve(lista);
        }

        public void ObrisiLogicki(string id)
        {
            var lista = GetSvi();
            var k = lista.FirstOrDefault(x => x.Id == id);
            if (k != null)
                k.Obrisan = true;
            SacuvajSve(lista);
        }
    }
}