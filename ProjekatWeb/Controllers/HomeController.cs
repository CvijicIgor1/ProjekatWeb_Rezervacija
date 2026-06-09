using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ProjekatWeb.Models;
using ProjekatWeb.Services;

namespace ProjekatWeb.Controllers
{
    public class HomeController : Controller
    {
        private ObjektiService objekatiService = new ObjektiService();

        public ActionResult Index(string naziv, string grad, string tip, double? cenaMin, double? cenaMax, string sortBy)
        {
            var objekti = objekatiService.GetSvi()
                .Where(o => o.Dostupnost && !o.Obrisan)
                .ToList();

            
            if (!string.IsNullOrEmpty(naziv))
                objekti = objekti.Where(o => o.Naziv.ToLower().Contains(naziv.ToLower())).ToList();

            if (!string.IsNullOrEmpty(grad))
                objekti = objekti.Where(o => o.Grad.ToLower().Contains(grad.ToLower())).ToList();

            if (!string.IsNullOrEmpty(tip))
                objekti = objekti.Where(o => o.Tip.ToLower() == tip.ToLower()).ToList();

            if (cenaMin.HasValue)
                objekti = objekti.Where(o => o.Cena_po_noci >= cenaMin.Value).ToList();

            if (cenaMax.HasValue)
                objekti = objekti.Where(o => o.Cena_po_noci <= cenaMax.Value).ToList();

            
            switch (sortBy)
            {
                case "naziv_asc":
                    objekti = objekti.OrderBy(o => o.Naziv).ToList(); break;
                case "naziv_desc":
                    objekti = objekti.OrderByDescending(o => o.Naziv).ToList(); break;
                case "cena_asc":
                    objekti = objekti.OrderBy(o => o.Cena_po_noci).ToList(); break;
                case "cena_desc":
                    objekti = objekti.OrderByDescending(o => o.Cena_po_noci).ToList(); break;
                case "datum_asc":
                    objekti = objekti.OrderBy(o => o.Datum_postavljanja_oglasa).ToList(); break;
                case "datum_desc":
                    objekti = objekti.OrderByDescending(o => o.Datum_postavljanja_oglasa).ToList(); break;
            }

            
            ViewBag.naziv = naziv;
            ViewBag.grad = grad;
            ViewBag.tip = tip;
            ViewBag.cenaMin = cenaMin;
            ViewBag.cenaMax = cenaMax;
            ViewBag.sortBy = sortBy;

            return View(objekti);
        }
    }
}