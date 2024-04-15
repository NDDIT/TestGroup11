using LuxuryHotel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LuxuryHotel.Areas.Reception.Controllers
{
    [Authorize]
    public class StatisticController : Controller
    {
        private dbDataContext db = new dbDataContext();

        
        public ActionResult Index()
        {
            return View();
        }
    }
}