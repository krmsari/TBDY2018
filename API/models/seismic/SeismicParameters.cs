namespace API.Models.Seismic
{
    /// <summary>
    /// Kullanıcı tarafından girilen TBDY-2018 deprem parametrelerini tutar.
    /// </summary>
    public class SeismicParameters
    {
        public double Ss { get; set; }
        public double S1 { get; set; }
        public string SiteClass { get; set; } // Örn: "ZA", "ZB", "ZC"
        public double R { get; set; } // Taşıyıcı Sistem Davranış Katsayısı
        public double D { get; set; } // Dayanım Fazlalığı Katsayısı
        public double I { get; set; } // Bina Önem Katsayısı
    }
}
