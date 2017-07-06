﻿
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TpIntegradorDiuj.Models;

namespace TpIntegradorDiuj.Controllers
{
    public class IndicadoresController : Controller
    {
        // GET: Indicadores
        EmpresasController empController = new EmpresasController();
        public ActionResult Index()
        {
            ViewBag.ListIndicadores = DeserializarArchivoIndicadores().Select(x => new SelectListItem {
                Text = x.Nombre,
                Value = x.Id.ToString()
            }).ToList();
            
            return View();
        }
        public List<Indicador> DeserializarArchivoIndicadores()
        {
            string buf = System.IO.File.ReadAllText(Server.MapPath("~/App_Data/Archivos/") + "indicadores.json");
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<Indicador> listIndicadores = serializer.Deserialize<List<Indicador>>(buf);
            return listIndicadores;

        }
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public JsonResult ObtenerFormulaDelIndicador(int idIndicador)
        {
            var indicador = DeserializarArchivoIndicadores().FirstOrDefault(x => x.Id == idIndicador);
            return Json(new { Formula = indicador.Formula });
        }
        [HttpPost]
        public ActionResult Create(Indicador model)
        {
            
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<Indicador>  indicadores=DeserializarArchivoIndicadores();
            int maxId = indicadores.Select(x => x.Id).Max();
            model.Id = maxId+1;
            //Valido que la formula sea sintacticamente correcta
            if (model.EsFormulaValida())
            {
                indicadores.Add(model);
                //Guardo el indicador en lel JSON
                string jsonData = JsonConvert.SerializeObject(indicadores);
                System.IO.File.WriteAllText(Server.MapPath("~/App_Data/Archivos/") + "indicadores.json", jsonData);
            }
            else
            {
                ModelState.AddModelError("", "La formula del indicador no es valida sintacticamente");
            }
           
            return RedirectToAction("Index");
        }
        public ActionResult EvaluarIndicadorParaEmpresa(int idIndicador,int idEmpresa,int periodo)
        {
            //Obtengo el indicador a aplicar
            var indicador = DeserializarArchivoIndicadores().FirstOrDefault(x => x.Id == idIndicador);
            //Obtengo la empresa, a la cual quiero aplicarle el indicador
            var empresa = empController.DeserializarArchivoEmpresas().FirstOrDefault(x => x.Id == idEmpresa);
            //Aplico el indicador, es decir, hay que parsear la formula
            double valorTrasAplicarIndicador = indicador.ObtenerValor(empresa, periodo);
            return Json(new { Valor = valorTrasAplicarIndicador });
        }
       
    }
}