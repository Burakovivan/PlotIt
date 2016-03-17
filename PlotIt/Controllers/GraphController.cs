using PlottingGraphsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PlotIt.Controllers
{
    public class GraphController : Controller
    {
        // GET: Graph
        public ActionResult Index(string expression)
        {
            return View();
        }


        [HttpPost]
        //public ActionResult Plot(string expression, int width, int height, float dimention)
        public ActionResult Plot(string expression)
        {
            float dimention = 20f;
            float[,] graph = Plotter.GetPointSchema(expression,-30, 30,-30,30, 1/dimention);
            return Json(graph);
        }

    }


}