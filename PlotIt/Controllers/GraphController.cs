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
        public ActionResult Plot(string expression)
        {
            int width = 200;
            int height = 200;

            float[,] graph = Plotter.GetPointSchema(expression, width, height);
            return Json(graph);
        }

    }


}