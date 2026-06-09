using System;
using System.Web;
using System.Web.Mvc;
using ProjekatWeb.Models;
using ProjekatWeb.Services;

namespace ProjekatWeb.Controllers
{
    public class KorisnikController : Controller
    {
        private KorisnikService korisnikService = new KorisnikService();


        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string korisnickoIme,string lozinka)
        {
            var korisnik = korisnikService.NadjiPoKorisnickomImenu(korisnickoIme);
            if (korisnik == null || korisnik.Lozinka != lozinka)
            {
                ViewBag.Greska = "Wrong password or username";
                return View();
            }

            Session["korisnik"] = korisnik.KorisnickoIme;
            Session["tip"] = korisnik.TipKorisnika.ToString();

            if (korisnik.TipKorisnika == TipKorisnika.Administrator)
                return RedirectToAction("Index", "Admin");
            else if (korisnik.TipKorisnika == TipKorisnika.Domacin)
                return RedirectToAction("Index", "Domacin");
            else
                return RedirectToAction("Index", "Home");
        }

        public ActionResult Registracija()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Registracija(string korisnickoIme, string lozinka, string ime, string prezime, string email, string datumRodjenja, string pol)
        {
            var postojeci = korisnikService.NadjiPoKorisnickomImenu(korisnickoIme);
            if (postojeci != null)
            {
                ViewBag.Greska = "User already exsists";
                return View();
            }

            var noviKorisnik = new Korisnik
            {
                KorisnickoIme = korisnickoIme,
                Lozinka = lozinka,
                Ime = ime,
                Prezime = prezime,
                Email = email,
                DatumRodjenja = datumRodjenja,
                Pol = pol,
                TipKorisnika = TipKorisnika.Gost,
                Obrisan = false
            };

            korisnikService.Dodaj(noviKorisnik);

            return RedirectToAction("Login");
        }

        public ActionResult Odjava()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        public ActionResult Profil()
        {
            if (Session["korisnik"]==null)
            {
                return RedirectToAction("Login");
            }

            var korisnik = korisnikService.NadjiPoKorisnickomImenu(Session["korisnik"].ToString());
            return View(korisnik);
        }

        [HttpPost]
        public ActionResult Profil(string ime, string prezime, string email, string datumRodjenja, string pol)
        {
            if (Session["korisnik"] == null)
                return RedirectToAction("Login");
            var korisnik = korisnikService.NadjiPoKorisnickomImenu(Session["korisnik"].ToString());
            korisnik.Ime = ime;
            korisnik.Prezime = prezime;
            korisnik.Email = email;
            korisnik.DatumRodjenja = datumRodjenja;
            korisnik.Pol = pol;

            korisnikService.Azuriraj(korisnik);
            ViewBag.Uspeh = "Profil uspešno ažuriran!";
            return View(korisnik);
        }
    }
}