using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ObligatorioDDA.src.Controllers
{
    public class JuegoController : Controller
    {
        // GET: JuegoController
        public ActionResult Index()
        {
            return View();
        }

        // GET: JuegoController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: JuegoController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: JuegoController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: JuegoController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: JuegoController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: JuegoController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: JuegoController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
