using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using SehirBolgeRenklendirme.Models;
using SehirBolgeRenklendirme.Logic;
using System.Text.Json;

namespace SehirBolgeRenklendirme.Controllers
{
    public class MapController : Controller
    {
        private readonly ColoringEngine _engine = new ColoringEngine();

        [HttpGet]
        public IActionResult GetAndColorDistricts(string cityName)
        {
            try
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "ilcelers.json");
                var jsonText = System.IO.File.ReadAllText(path);
                var options = new JsonSerializerOptions();
                options.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());
                var featureCollection = JsonSerializer.Deserialize<FeatureCollection>(jsonText, options);

                // GADM: NAME_1 ile ili süzüyoruz
                var selectedFeatures = featureCollection.Where(f =>
                    f.Attributes["NAME_1"]?.ToString().Equals(cityName, StringComparison.OrdinalIgnoreCase) == true).ToList();

                var districtList = new List<District>();

                // Komşuluk Analizi
                foreach (var fA in selectedFeatures)
                {
                    var d = new District
                    {
                        Name = fA.Attributes["NAME_2"].ToString(),
                        Neighbors = new List<string>(),
                        ColorId = 0
                    };

                    foreach (var fB in selectedFeatures)
                    {
                        if (fA == fB) continue;

                        // KRİTİK: Buffer(0.0001) ekleyerek, sınırları tam öpüşmeyen 
                        // ilçeleri de 'komşu' olarak yakalıyoruz. 
                        // Bu, iki ilçenin aynı renk (yeşil-yeşil) olmasını engeller.
                        if (fA.Geometry.Touches(fB.Geometry) ||
                            fA.Geometry.Intersects(fB.Geometry.Buffer(0.0001)))
                        {
                            d.Neighbors.Add(fB.Attributes["NAME_2"].ToString());
                        }
                    }
                    districtList.Add(d);
                }

                _engine.IsPlanar(districtList);
                _engine.Solve(districtList, 0);

                return Json(new
                {
                    districts = districtList,
                    planarResult = _engine.IsPlanar(districtList),
                    stats = new
                    {
                        v = districtList.Count,
                        e = districtList.Sum(d => d.Neighbors.Count) / 2,
                        f = (districtList.Sum(d => d.Neighbors.Count) / 2) - districtList.Count + 2
                    }
                });
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }
    }
}