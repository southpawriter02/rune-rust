namespace RuneAndRust.Core;

public class Attributes
{
    public int Might { get; set; }
    public int Finesse { get; set; }
    public int Wits { get; set; }
    public int Will { get; set; }
    public int Sturdiness { get; set; }

    public Attributes() { }

    public Attributes(int might, int finesse, int wits, int will, int sturdiness)
    {
        Might = might;
        Finesse = finesse;
        Wits = wits;
        Will = will;
        Sturdiness = sturdiness;
    }
}
