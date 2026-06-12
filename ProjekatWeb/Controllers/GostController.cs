using System;
using System.Linq;
using System.Web.Mvc;
using ProjekatWeb.Models;
using ProjekatWeb.Services;


namespace ProjekatWeb.Controllers
{
    public class GostController : Controller
    {
        private KorisnikService korisnikService = new KorisnikService();
        private ObjektiService objekatiService = new ObjektiService();
        private RezervacijaService rezervacijaService = new RezervacijaService();
        private RecenzijaService recenzijaService = new RecenzijaService();


        private bool IsGost() 
        {
            return Session["korisnik"] != null && Session["tip"].ToString() == "Gost";
        }

        public ActionResult Profil()
        {
            if (!IsGost()) 
                return RedirectToAction("Login", "Korisnik");
            var korisnik = korisnikService.NadjiPoKorisnickomImenu(Session["korisnik"].ToString());
            return View(korisnik);
        }

        [HttpPost]
        public ActionResult Profil(string ime, string prezime, string email,string datumRodjenja, string pol)
        {
            if (!IsGost()) 
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

        public ActionResult Rezervacije(string status)
        {
            if (!IsGost()) 
                return RedirectToAction("Login", "Korisnik");

            var rezervacije = rezervacijaService.GetZaGosta(Session["korisnik"].ToString());

            if (!string.IsNullOrEmpty(status))
                rezervacije = rezervacije.Where(r => r.Status.ToString() == status).ToList();

            ViewBag.status = status;
            return View(rezervacije);
        }

        public ActionResult KreirajRezervaciju(string objekatId)
        {
            if (!IsGost()) 
                return RedirectToAction("Login", "Korisnik");

            var objekat = objekatiService.NadjiPoId(objekatId);
            if (objekat == null) return RedirectToAction("Index", "Home");

            return View(objekat);
        }

        [HttpPost]
        public ActionResult KreirajRezervaciju(string objekatId, string datumPrijave,
            string datumOdjave, int brojGostiju)
        {
            if (!IsGost()) 
                return RedirectToAction("Login", "Korisnik");

            var objekat = objekatiService.NadjiPoId(objekatId);

           
            if (brojGostiju > objekat.Maksimalan_broj_gostiju)
            {
                ViewBag.Greska = "Number of guests exceeds the maximum allowed (" + objekat.Maksimalan_broj_gostiju + ")!";
                return View(objekat);
            }

            
            DateTime checkIn = DateTime.ParseExact(datumPrijave, "dd/MM/yyyy", null);
            DateTime checkOut = DateTime.ParseExact(datumOdjave, "dd/MM/yyyy", null);

            if (checkOut <= checkIn)
            {
                ViewBag.Greska = "Check-out date must be after check-in date!";
                return View(objekat);
            }

           
            var postojeceRezervacije = rezervacijaService.GetSve()
                .Where(r => r.SmestajniObjekatId == objekatId
                    && r.Status == StatusRezervacije.Odobrena
                    && !r.Obrisana)
                .ToList();

            foreach (var r in postojeceRezervacije)
            {
                DateTime exCheckIn = DateTime.ParseExact(r.DatumPrijave, "dd/MM/yyyy", null);
                DateTime exCheckOut = DateTime.ParseExact(r.DatumOdjave, "dd/MM/yyyy", null);

                if (checkIn < exCheckOut && checkOut > exCheckIn)
                {
                    ViewBag.Greska = "Selected dates overlap with an existing reservation!";
                    return View(objekat);
                }
            }

            
            int brojNoci = (int)(checkOut - checkIn).TotalDays;
            double ukupnaCena = brojNoci * objekat.Cena_po_noci;

           
            var rezervacija = new Rezervacija
            {
                Id = Guid.NewGuid().ToString(),
                SmestajniObjekatId = objekatId,
                Gost = Session["korisnik"].ToString(),
                DatumPrijave = datumPrijave,
                DatumOdjave = datumOdjave,
                BrojGostiju = brojGostiju,
                UkupnaCena = ukupnaCena,
                Status = StatusRezervacije.Kreirana,
                Obrisana = false
            };

            rezervacijaService.Dodaj(rezervacija);

            
            var korisnik = korisnikService.NadjiPoKorisnickomImenu(Session["korisnik"].ToString());
            korisnik.RezervacijaIds.Add(rezervacija.Id);
            korisnikService.Azuriraj(korisnik);

            return RedirectToAction("Rezervacije");
        }

        [HttpPost]
        public ActionResult OtkaziRezervaciju(string id)
        {
            if (!IsGost()) return RedirectToAction("Login", "Korisnik");

            var rezervacija = rezervacijaService.NadjiPoId(id);
            if (rezervacija == null || rezervacija.Gost != Session["korisnik"].ToString())
                return RedirectToAction("Rezervacije");

            
            if (rezervacija.Status != StatusRezervacije.Kreirana && rezervacija.Status != StatusRezervacije.Odobrena)
            {
                TempData["Greska"] = "This reservation cannot be cancelled!";
                return RedirectToAction("Rezervacije");
            }

            
            DateTime checkIn = DateTime.ParseExact(rezervacija.DatumPrijave, "dd/MM/yyyy", null);
            if ((checkIn - DateTime.Now).TotalHours < 24)
            {
                TempData["Greska"] = "Reservation can only be cancelled at least 24 hours before check-in!";
                return RedirectToAction("Rezervacije");
            }

            rezervacija.Status = StatusRezervacije.Otkazana;
            rezervacijaService.Azuriraj(rezervacija);

            return RedirectToAction("Rezervacije");
        }

        public ActionResult DodajRecenziju(string objekatId)
        {
            if (!IsGost()) return RedirectToAction("Login", "Korisnik");

            var objekat = objekatiService.NadjiPoId(objekatId);
            if (objekat == null) 
                return RedirectToAction("Rezervacije");

            
            var zavrsena = rezervacijaService.GetZaGosta(Session["korisnik"].ToString()).Any(r => r.SmestajniObjekatId == objekatId && r.Status == StatusRezervacije.Zavrsena);

            if (!zavrsena)
            {
                TempData["Greska"] = "You can only review properties you have stayed at!";
                return RedirectToAction("Rezervacije");
            }

            ViewBag.ObjekatId = objekatId;
            ViewBag.ObjekatNaziv = objekat.Naziv;
            return View();
        }

        [HttpPost]
        public ActionResult DodajRecenziju(string objekatId, string naslov,
            string sadrzaj, int ocena)
        {
            if (!IsGost()) return RedirectToAction("Login", "Korisnik");

            var recenzija = new Recenzija
            {
                Id = Guid.NewGuid().ToString(),
                SmestajniObjekatId = objekatId,
                RecenzentKorisnickoIme = Session["korisnik"].ToString(),
                Naslov = naslov,
                Sadrzaj = sadrzaj,
                Ocena = ocena,
                Status = StatusRecenzije.Kreirana,
                Obrisana = false
            };

            recenzijaService.Dodaj(recenzija);

            var objekat = objekatiService.NadjiPoId(objekatId);
            objekat.RecenzijaIds.Add(recenzija.Id);
            objekatiService.Azuriraj(objekat);

            return RedirectToAction("Rezervacije");
        }

        [HttpPost]
        public ActionResult ObrisiRecenziju(string id)
        {
            if (!IsGost()) return RedirectToAction("Login", "Korisnik");

            var recenzija = recenzijaService.NadjiPoId(id);
            if (recenzija != null && recenzija.RecenzentKorisnickoIme == Session["korisnik"].ToString())
                recenzijaService.ObrisiLogicki(id);
            return RedirectToAction("Rezervacije");
        }
        public ActionResult IzmeniRecenziju(string id)
        {
            if (!IsGost()) return RedirectToAction("Login", "Korisnik");

            var recenzija = recenzijaService.NadjiPoId(id);
            if (recenzija == null || recenzija.RecenzentKorisnickoIme != Session["korisnik"].ToString())
                return RedirectToAction("Rezervacije");

            var objekat = objekatiService.NadjiPoId(recenzija.SmestajniObjekatId);
            ViewBag.ObjekatNaziv = objekat != null ? objekat.Naziv : "Property";

            return View(recenzija);
        }

        [HttpPost]
        public ActionResult IzmeniRecenziju(string id, string naslov, string sadrzaj, int ocena)
        {
            if (!IsGost()) return RedirectToAction("Login", "Korisnik");

            var recenzija = recenzijaService.NadjiPoId(id);
            if (recenzija == null || recenzija.RecenzentKorisnickoIme != Session["korisnik"].ToString())
                return RedirectToAction("Rezervacije");

            recenzija.Naslov = naslov;
            recenzija.Sadrzaj = sadrzaj;
            recenzija.Ocena = ocena;
            recenzija.Status = StatusRecenzije.Kreirana;

            recenzijaService.Azuriraj(recenzija);
            return RedirectToAction("Rezervacije");
        }

    }
}