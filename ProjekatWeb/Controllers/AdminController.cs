using System;
using System.Linq;
using System.Web.Mvc;
using ProjekatWeb.Models;
using ProjekatWeb.Services;

namespace ProjekatWeb.Controllers
{
    public class AdminController : Controller
    {
        private KorisnikService korisnikService = new KorisnikService();
        private ObjektiService objekatiService = new ObjektiService();
        private RezervacijaService rezervacijaService = new RezervacijaService();
        private RecenzijaService recenzijaService = new RecenzijaService();

        private bool IsAdmin()
        {
            return Session["korisnik"] != null && Session["tip"].ToString() == "Administrator";
        }


        public ActionResult Profil()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Korisnik");
            var korisnik = korisnikService.NadjiPoKorisnickomImenu(Session["korisnik"].ToString());
            return View(korisnik);
        }

        [HttpPost]
        public ActionResult Profil(string ime, string prezime, string email,
            string datumRodjenja, string pol)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Korisnik");
            var korisnik = korisnikService.NadjiPoKorisnickomImenu(Session["korisnik"].ToString());
            korisnik.Ime = ime;
            korisnik.Prezime = prezime;
            korisnik.Email = email;
            korisnik.DatumRodjenja = datumRodjenja;
            korisnik.Pol = pol;
            korisnikService.Azuriraj(korisnik);
            ViewBag.Uspeh = "Profile updated!";
            return View(korisnik);
        }

      

        public ActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Korisnik");
            ViewBag.BrojKorisnika = korisnikService.GetSvi().Count(k => !k.Obrisan && k.TipKorisnika != TipKorisnika.Administrator);
            ViewBag.BrojObjekta = objekatiService.GetSvi().Count(o => !o.Obrisan);
            ViewBag.BrojRezervacija = rezervacijaService.GetSve().Count(r => !r.Obrisana);
            ViewBag.BrojRecenzija = recenzijaService.GetSve().Count(r => !r.Obrisana && r.Status == StatusRecenzije.Kreirana);
            return View();
        }

       

        public ActionResult Korisnici(string ime, string prezime, string datumOd,
            string datumDo, string uloga, string sortBy)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Korisnik");

            var korisnici = korisnikService.GetSvi()
                .Where(k => !k.Obrisan && k.TipKorisnika != TipKorisnika.Administrator)
                .ToList();

           
            if (!string.IsNullOrEmpty(ime))
                korisnici = korisnici.Where(k => k.Ime.ToLower().Contains(ime.ToLower())).ToList();
            if (!string.IsNullOrEmpty(prezime))
                korisnici = korisnici.Where(k => k.Prezime.ToLower().Contains(prezime.ToLower())).ToList();
            if (!string.IsNullOrEmpty(uloga))
                korisnici = korisnici.Where(k => k.TipKorisnika.ToString() == uloga).ToList();
            if (!string.IsNullOrEmpty(datumOd))
            {
                DateTime od = DateTime.ParseExact(datumOd, "dd/MM/yyyy", null);
                korisnici = korisnici.Where(k => DateTime.ParseExact(k.DatumRodjenja, "dd/MM/yyyy", null) >= od).ToList();
            }
            if (!string.IsNullOrEmpty(datumDo))
            {
                DateTime do_ = DateTime.ParseExact(datumDo, "dd/MM/yyyy", null);
                korisnici = korisnici.Where(k => DateTime.ParseExact(k.DatumRodjenja, "dd/MM/yyyy", null) <= do_).ToList();
            }

            
            switch (sortBy)
            {
                case "ime_asc": korisnici = korisnici.OrderBy(k => k.Ime).ToList(); break;
                case "ime_desc": korisnici = korisnici.OrderByDescending(k => k.Ime).ToList(); break;
                case "datum_asc": korisnici = korisnici.OrderBy(k => DateTime.ParseExact(k.DatumRodjenja, "dd/MM/yyyy", null)).ToList(); break;
                case "datum_desc": korisnici = korisnici.OrderByDescending(k => DateTime.ParseExact(k.DatumRodjenja, "dd/MM/yyyy", null)).ToList(); break;
                case "uloga_asc": korisnici = korisnici.OrderBy(k => k.TipKorisnika).ToList(); break;
                case "uloga_desc": korisnici = korisnici.OrderByDescending(k => k.TipKorisnika).ToList(); break;
            }

            ViewBag.ime = ime; ViewBag.prezime = prezime;
            ViewBag.datumOd = datumOd; ViewBag.datumDo = datumDo;
            ViewBag.uloga = uloga; ViewBag.sortBy = sortBy;
            return View(korisnici);
        }

        
        public ActionResult DodajDomacina()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Korisnik");
            return View();
        }

        [HttpPost]
        public ActionResult DodajDomacina(string korisnickoIme, string lozinka, string ime,
            string prezime, string email, string datumRodjenja, string pol)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Korisnik");

            if (korisnikService.NadjiPoKorisnickomImenu(korisnickoIme) != null)
            {
                ViewBag.Greska = "Username already exists!";
                return View();
            }

            var domacin = new Korisnik
            {
                KorisnickoIme = korisnickoIme,
                Lozinka = lozinka,
                Ime = ime,
                Prezime = prezime,
                Email = email,
                DatumRodjenja = datumRodjenja,
                Pol = pol,
                TipKorisnika = TipKorisnika.Domacin,
                Obrisan = false
            };

            korisnikService.Dodaj(domacin);
            return RedirectToAction("Korisnici");
        }

        
        public ActionResult IzmeniKorisnika(string korisnickoIme)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Korisnik");
            var k = korisnikService.NadjiPoKorisnickomImenu(korisnickoIme);
            if (k == null) return RedirectToAction("Korisnici");
            return View(k);
        }

        [HttpPost]
        public ActionResult IzmeniKorisnika(string korisnickoIme, string lozinka, string ime,
            string prezime, string email, string datumRodjenja, string pol)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Korisnik");
            var k = korisnikService.NadjiPoKorisnickomImenu(korisnickoIme);
            k.Lozinka = lozinka;
            k.Ime = ime;
            k.Prezime = prezime;
            k.Email = email;
            k.DatumRodjenja = datumRodjenja;
            k.Pol = pol;
            korisnikService.Azuriraj(k);
            return RedirectToAction("Korisnici");
        }

        
        [HttpPost]
        public ActionResult ObrisiKorisnika(string korisnickoIme)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Korisnik");

            var k = korisnikService.NadjiPoKorisnickomImenu(korisnickoIme);
            if (k == null) return RedirectToAction("Korisnici");

            
            if (k.TipKorisnika == TipKorisnika.Gost)
            {
                var rezervacije = rezervacijaService.GetSve()
                    .Where(r => r.Gost == korisnickoIme && (r.Status == StatusRezervacije.Kreirana || r.Status == StatusRezervacije.Odobrena) && !r.Obrisana).ToList();

                foreach (var r in rezervacije)
                {
                    r.Status = StatusRezervacije.Otkazana;
                    rezervacijaService.Azuriraj(r);
                }
            }

            korisnikService.ObrisiLogicki(korisnickoIme);
            return RedirectToAction("Korisnici");
        }

        public ActionResult Objekti(string dostupnost, string sortBy)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Korisnik");

            var objekti = objekatiService.GetSvi().Where(o => !o.Obrisan).ToList();

            if (dostupnost == "dostupni")
                objekti = objekti.Where(o => o.Dostupnost).ToList();
            else if (dostupnost == "nedostupni")
                objekti = objekti.Where(o => !o.Dostupnost).ToList();

            switch (sortBy)
            {
                case "naziv_asc": objekti = objekti.OrderBy(o => o.Naziv).ToList(); break;
                case "naziv_desc": objekti = objekti.OrderByDescending(o => o.Naziv).ToList(); break;
                case "cena_asc": objekti = objekti.OrderBy(o => o.Cena_po_noci).ToList(); break;
                case "cena_desc": objekti = objekti.OrderByDescending(o => o.Cena_po_noci).ToList(); break;
                case "datum_asc": objekti = objekti.OrderBy(o => o.Datum_postavljanja_oglasa).ToList(); break;
                case "datum_desc": objekti = objekti.OrderByDescending(o => o.Datum_postavljanja_oglasa).ToList(); break;
            }

            ViewBag.dostupnost = dostupnost;
            ViewBag.sortBy = sortBy;
            return View(objekti);
        }

        public ActionResult IzmeniObjekat(string id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Korisnik");
            var o = objekatiService.NadjiPoId(id);
            if (o == null) return RedirectToAction("Objekti");
            return View(o);
        }

        [HttpPost]
        public ActionResult IzmeniObjekat(string id, string naziv, string tip, string opis,
            string adresa, string grad, double cena, int maxGostiju, bool dostupnost)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Korisnik");
            var o = objekatiService.NadjiPoId(id);
            o.Naziv = naziv; 
            o.Tip = tip; 
            o.Opis = opis;
            o.Adresa = adresa; 
            o.Grad = grad;
            o.Cena_po_noci = cena; 
            o.Maksimalan_broj_gostiju = maxGostiju;
            o.Dostupnost = dostupnost;
            objekatiService.Azuriraj(o);
            return RedirectToAction("Objekti");
        }

        [HttpPost]
        public ActionResult ObrisiObjekat(string id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Korisnik");

            var aktivne = rezervacijaService.GetSve()
                .Any(r => r.SmestajniObjekatId == id
                    && (r.Status == StatusRezervacije.Kreirana || r.Status == StatusRezervacije.Odobrena)
                    && !r.Obrisana);

            if (aktivne)
            {
                TempData["Greska"] = "Cannot delete property with active reservations!";
                return RedirectToAction("Objekti");
            }

            objekatiService.ObrisiLogicki(id);
            return RedirectToAction("Objekti");
        }

        public ActionResult Rezervacije()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Korisnik");
            var rezervacije = rezervacijaService.GetSve().Where(r => !r.Obrisana).ToList();
            return View(rezervacije);
        }

        [HttpPost]
        public ActionResult OdobriRezervaciju(string id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Korisnik");
            var r = rezervacijaService.NadjiPoId(id);
            if (r != null && r.Status == StatusRezervacije.Kreirana)
            {
                r.Status = StatusRezervacije.Odobrena;
                rezervacijaService.Azuriraj(r);
            }
            return RedirectToAction("Rezervacije");
        }

        [HttpPost]
        public ActionResult OtkaziRezervaciju(string id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Korisnik");
            var r = rezervacijaService.NadjiPoId(id);
            if (r == null) return RedirectToAction("Rezervacije");

            if (r.Status != StatusRezervacije.Kreirana && r.Status != StatusRezervacije.Odobrena)
            {
                TempData["Greska"] = "Only Created or Approved reservations can be cancelled!";
                return RedirectToAction("Rezervacije");
            }

            DateTime checkIn = DateTime.ParseExact(r.DatumPrijave, "dd/MM/yyyy", null);
            if ((checkIn - DateTime.Now).TotalHours < 24)
            {
                TempData["Greska"] = "Cannot cancel less than 24h before check-in!";
                return RedirectToAction("Rezervacije");
            }

            r.Status = StatusRezervacije.Otkazana;
            rezervacijaService.Azuriraj(r);
            return RedirectToAction("Rezervacije");
        }

        public ActionResult Recenzije()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Korisnik");
            var recenzije = recenzijaService.GetSve().Where(r => !r.Obrisana).ToList();
            return View(recenzije);
        }

        [HttpPost]
        public ActionResult OdobriRecenziju(string id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Korisnik");
            var r = recenzijaService.NadjiPoId(id);
            if (r != null)
            {
                r.Status = StatusRecenzije.Odobrena;
                recenzijaService.Azuriraj(r);
            }
            return RedirectToAction("Recenzije");
        }

        [HttpPost]
        public ActionResult OdbijRecenziju(string id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Korisnik");
            var r = recenzijaService.NadjiPoId(id);
            if (r != null)
            {
                r.Status = StatusRecenzije.Odbijena;
                recenzijaService.Azuriraj(r);
            }
            return RedirectToAction("Recenzije");
        }
    }
}