using SehirBolgeRenklendirme.Models;

namespace SehirBolgeRenklendirme.Logic
{
    public class ColoringEngine
    {

        public bool IsPlanar(List<District> districts)
        {
            int V = districts.Count;
            int E = districts.Sum(d => d.Neighbors.Count) / 2;
            int F = E - V + 2;
            return (V - E + F) == 2;
        }

        
        public bool Solve(List<District> districts, int index)
        {
            if (index == districts.Count) return true;

            for (int color = 1; color <= 4; color++)
            {
                if (IsSafe(districts[index], color, districts))
                {
                    districts[index].ColorId = color;
                    if (Solve(districts, index + 1)) return true;
                    districts[index].ColorId = 0; 
                }
            }
            return false;
        }

        private bool IsSafe(District district, int color, List<District> all)
        {
            foreach (var nName in district.Neighbors)
            {
                var neighbor = all.FirstOrDefault(d => d.Name == nName);
                if (neighbor != null && neighbor.ColorId == color) return false;
            }
            return true;
        }

    }
}
