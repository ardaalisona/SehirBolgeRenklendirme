namespace SehirBolgeRenklendirme.Models
{
    public class District
    {
        public string Name { get; set; }
        public List<string> Neighbors { get; set; } = new List<string>();
        public int ColorId { get; set; }


    }
}
