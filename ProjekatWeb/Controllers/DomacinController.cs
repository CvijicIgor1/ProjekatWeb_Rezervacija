using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProjekatWeb.Models;
using ProjekatWeb.Services;

namespace ProjekatWeb.Controllers
{
    public class DomacinController : Controller
    {
        private KorisnikService korisnikService = new KorisnikService();
        private ObjektiService objektiService = new ObjektiService();
        private RezervacijaService rezervacijaService = new RezervacijaService();

        private bool isDomacin()
        {
            return Session["korisnik"] != null && Session["tip"].ToString() == "Domacin";
        }

        public ActionResult Profil()
        {
            if (!isDomacin()) 
                return RedirectToAction("Login", "Korisnik");
            var korisnik = korisnikService.NadjiPoKorisnickomImenu(Session["korisnik"].ToString());
            return View(korisnik);
        }

        [HttpPost]
        public ActionResult Profil(string ime, string prezime, string email, string datumRodjenja, string pol)
        {
            if (!isDomacin()) 
                return RedirectToAction("Login", "Korisnik");
            var korisnik = korisnikService.NadjiPoKorisnickomImenu(Session["korisnik"].ToString());
            korisnik.Ime = ime;
            korisnik.Prezime = prezime;
            korisnik.Email = email;
            korisnik.DatumRodjenja = datumRodjenja;
            korisnik.Pol = pol;
            korisnikService.Azuriraj(korisnik);
            ViewBag.Uspeh = "Profile updated successfully!";
            return View(korisnik);
        }

        public ActionResult Index(string dostupnost, string sortBy)
        {
            if (!isDomacin()) return RedirectToAction("Login", "Korisnik");

            var sviObjekti = objektiService.GetSvi().Where(o => o.DomacinKorisnickoIme == Session["korisnik"].ToString() && !o.Obrisan).ToList();

            
            if (dostupnost == "dostupni")
                sviObjekti = sviObjekti.Where(o => o.Dostupnost).ToList();
            else if (dostupnost == "nedostupni")
                sviObjekti = sviObjekti.Where(o => !o.Dostupnost).ToList();

            
            switch (sortBy)
            {
                case "naziv_asc": sviObjekti = sviObjekti.OrderBy(o => o.Naziv).ToList(); break;
                case "naziv_desc": sviObjekti = sviObjekti.OrderByDescending(o => o.Naziv).ToList(); break;
                case "cena_asc": sviObjekti = sviObjekti.OrderBy(o => o.Cena_po_noci).ToList(); break;
                case "cena_desc": sviObjekti = sviObjekti.OrderByDescending(o => o.Cena_po_noci).ToList(); break;
                case "datum_asc": sviObjekti = sviObjekti.OrderBy(o => o.Datum_postavljanja_oglasa).ToList(); break;
                case "datum_desc": sviObjekti = sviObjekti.OrderByDescending(o => o.Datum_postavljanja_oglasa).ToList(); break;
            }

            ViewBag.dostupnost = dostupnost;
            ViewBag.sortBy = sortBy;
            return View(sviObjekti);
        }

        public ActionResult DodajObjekat()
        {
            if (!isDomacin()) return RedirectToAction("Login", "Korisnik");
            return View();
        }

        [HttpPost]
        public ActionResult DodajObjekat(string naziv, string tip, string opis,string adresa, string grad, double cena, int maxGostiju, bool dostupnost,HttpPostedFileBase slika)
        {
            if (!isDomacin()) return RedirectToAction("Login", "Korisnik");

            
            if (slika == null || slika.ContentLength == 0)
            {
                ViewBag.Greska = "Image is required!";
                return View();
            }

            string slikaPath = "";
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(slika.FileName);
            string folderPath = Server.MapPath("~/Images/");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            slika.SaveAs(folderPath + fileName);
            slikaPath = "/Images/" + fileName;

            var objekat = new Smestajni_objekat
            {
                Id = Guid.NewGuid().ToString(),
                Naziv = naziv,
                Tip = tip,
                Opis = opis,
                Adresa = adresa,
                Grad = grad,
                Cena_po_noci = cena,
                Maksimalan_broj_gostiju = maxGostiju,
                Slika = slikaPath,
                Datum_postavljanja_oglasa = DateTime.Now.ToString("dd/MM/yyyy"),
                Dostupnost = dostupnost,
                Obrisan = false,
                DomacinKorisnickoIme = Session["korisnik"].ToString()
            };

            objektiService.Dodaj(objekat);

            
            var korisnik = korisnikService.NadjiPoKorisnickomImenu(Session["korisnik"].ToString());
            korisnik.ObjektiIds.Add(objekat.Id);
            korisnikService.Azuriraj(korisnik);

            return RedirectToAction("Index");
        }

        public ActionResult IzmeniObjekat(string id)
        {
            if (!isDomacin()) return RedirectToAction("Login", "Korisnik");
            var objekat = objektiService.NadjiPoId(id);
            if (objekat == null || objekat.DomacinKorisnickoIme != Session["korisnik"].ToString())
                return RedirectToAction("Index");
            if (!objekat.Dostupnost)
            {
                TempData["Greska"] = "Unavailable properties cannot be edited!";
                return RedirectToAction("Index");
            }
            return View(objekat);
        }

        [HttpPost]
        public ActionResult IzmeniObjekat(string id, string naziv, string tip, string opis,string adresa, string grad, double cena, int maxGostiju, bool dostupnost,HttpPostedFileBase slika)
        {
            if (!isDomacin()) return RedirectToAction("Login", "Korisnik");

            var objekat = objektiService.NadjiPoId(id);
            if (objekat == null) return RedirectToAction("Index");

            objekat.Naziv = naziv;
            objekat.Tip = tip;
            objekat.Opis = opis;
            objekat.Adresa = adresa;
            objekat.Grad = grad;
            objekat.Cena_po_noci = cena;
            objekat.Maksimalan_broj_gostiju = maxGostiju;
            objekat.Dostupnost = dostupnost;

            
            if (slika != null && slika.ContentLength > 0)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(slika.FileName);
                string folderPath = Server.MapPath("~/Images/");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
                slika.SaveAs(folderPath + fileName);
                objekat.Slika = "/Images/" + fileName;
            }

            objektiService.Azuriraj(objekat);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ObrisiObjekat(string id)
        {
            if (!isDomacin()) return RedirectToAction("Login", "Korisnik");

            var objekat = objektiService.NadjiPoId(id);
            if (objekat == null) return RedirectToAction("Index");

            if (!objekat.Dostupnost)
            {
                TempData["Greska"] = "Unavailable";
                return RedirectToAction("Index");
            }

            var aktivneRezervacije = rezervacijaService.GetSve().Any(r => r.SmestajniObjekatId == id && (r.Status == StatusRezervacije.Kreirana || r.Status == StatusRezervacije.Odobrena) && !r.Obrisana);

            if (aktivneRezervacije)
            {
                TempData["Greska"] = "Cannot delete property with active reservations";
                return RedirectToAction("Index");
            }

            objektiService.ObrisiLogicki(id);
            return RedirectToAction("Index");
        }
    }
}