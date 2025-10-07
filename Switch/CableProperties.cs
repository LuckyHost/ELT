namespace ElectroTools
{
    public class CableProperties
    {
        public string Name { get; internal set; }
        public double R { get; set; }
        public double X { get; set; }
        public double R0 { get; set; }
        public double X0 { get; set; }
        public double RN { get; set; }
        public double XN { get; set; }
        public double Ke { get; set; }
        public double Ce { get; set; }
        public bool IsDefault { get; set; }
        public double Icrict { get; set; }
    }
}