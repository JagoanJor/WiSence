namespace API.Responses
{
    public class TotalResponse
    {
        public int Total { get; set; }
        public TotalResponse(int total)
        {
            Total = total;
        }
    }

    public class JumlahKehadiranHariIni
    {
        public int Ontime { get; set; }
        public int Terlambat { get; set; }
        public int Cuti { get; set; }
        public int Absen { get; set; }
        public JumlahKehadiranHariIni(int ontime, int terlambat, int cuti, int absen)
        {
            Ontime = ontime;
            Terlambat = terlambat;
            Cuti = cuti;
            Absen = absen;
        }
    }
}
